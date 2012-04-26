package com.tol1.irssinotifier.server;

import java.io.IOException;
import java.io.OutputStreamWriter;
import java.net.HttpURLConnection;
import java.net.URL;
import java.net.URLDecoder;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.tol1.irssinotifier.server.datamodels.IrssiNotifierUser;
import com.tol1.irssinotifier.server.datamodels.Message;
import com.tol1.irssinotifier.server.exception.UserNotFoundException;
import com.tol1.irssinotifier.server.utils.ObjectifyDAO;

@SuppressWarnings("serial")
public class MessageHandler extends HttpServlet {

	@Override
	protected void doPost(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		String id = req.getParameter("apiToken").trim();
		
		ObjectifyDAO dao = new ObjectifyDAO();
		
		try {
			IrssiNotifierUser user = IrssiNotifier.getUser(dao, id);
			
			URL url = new URL(user.ChannelURI);
			
			String nick = URLDecoder.decode(req.getParameter("nick"),"UTF-8");
			String channel = URLDecoder.decode(req.getParameter("channel"),"UTF-8");
			String message = URLDecoder.decode(req.getParameter("message"),"UTF-8");
			
			Message mess = new Message(nick, channel, message, user);
			
			if(user.sendToastNotifications){
				String toastMessage = mess.GenerateToastNotification();
				HttpURLConnection conn = DoSend(toastMessage,"toast","2",url);
				Status responseStatus = HandleResponse(conn, resp, user, dao);
				if(responseStatus == Status.STATUS_OK) {
					IrssiNotifier.log.info("Toast notification lähetetty onnistuneesti");
				} else {
					IrssiNotifier.log.warning("Toast notificationin lähetyksessä virhe, tulos: "+responseStatus);
					//error
				}
			}
			if(user.sendTileNotifications){
				String tileMessage = mess.GenerateTileNotification(user.tileCount+1, IrssiNotifier.HILITEPAGEURL+"?NavigatedFrom=Tile");
				HttpURLConnection conn = DoSend(tileMessage,"token","1",url);
				Status responseStatus = HandleResponse(conn, resp, user, dao);
				IrssiNotifier.log.info("Tile notification lähetetty, tulos: "+responseStatus);
				if(responseStatus == Status.STATUS_OK){
					IrssiNotifier.log.info("Tile notification lähetetty onnistuneesti, päivitetään count");
					user.tileCount++;
					dao.ofy().put(user);
				}
				else{
					IrssiNotifier.log.warning("Tile notificationin lähetyksessä virhe, tulos: "+responseStatus);
				}
			}
			
			if(user.sendTileNotifications || user.sendToastNotifications){
				dao.ofy().put(mess);
			}
		} catch (UserNotFoundException e) {
			IrssiNotifier.printError(resp.getWriter(), e.getLocalizedMessage());
		}
	}
	
	public static HttpURLConnection DoSend(String payload, String type, String notificationClass, URL url) throws IOException{
		HttpURLConnection conn = (HttpURLConnection)url.openConnection();
		conn.setRequestMethod("POST");
		conn.setDoOutput(true);
		conn.setUseCaches(false);
		conn.setRequestProperty("Content-Type", "text/xml");
		conn.setRequestProperty("X-WindowsPhone-Target", type);
		conn.setRequestProperty("X-NotificationClass", notificationClass);

		OutputStreamWriter writer = null;
		try {
			writer = new OutputStreamWriter(conn.getOutputStream(), "UTF-8");
			writer.write(payload);
		} finally {
			if (writer != null)
				try {
					writer.close();
				} catch (IOException logOrIgnore) {
				}
		}
		return conn;
	}
	
	public static Status HandleResponse(HttpURLConnection conn, HttpServletResponse resp, IrssiNotifierUser user, ObjectifyDAO dao) throws IOException{
		Status result;
		int status = conn.getResponseCode();
		String NotificationStatus = "n/a";
		String DeviceConnectionStatus = "n/a";
		String SubscriptionStatus = "n/a";
		try{
			NotificationStatus = conn.getHeaderField("X-NotificationStatus");
			DeviceConnectionStatus = conn.getHeaderField("X-DeviceConnectionStatus");
			SubscriptionStatus = conn.getHeaderField("X-SubscriptionStatus");
		}
		catch(ArrayIndexOutOfBoundsException aioobe){}
		
		switch(status){
		case 200:
			if(NotificationStatus.equalsIgnoreCase("Received")){
				result = Status.STATUS_OK;
			}
			else if(NotificationStatus.equalsIgnoreCase("QueueFull")){
				resp.getWriter().println("Push channel error: Queue overflow.");
				result = Status.STATUS_QUEUEABLE;
			}
			else{
				resp.getWriter().println("Push channel error: Suppressed.");
				result = Status.STATUS_ERROR;
			}
			break;
		case 400:
			resp.getWriter().println("Push channel error: Bad XML.");
			result = Status.STATUS_ERROR;
			break;
		case 404:
			user.sendToastNotifications = false;
			dao.ofy().put(user);
			resp.getWriter().println("Push channel error: The subscription is invalid and is not present on the Push Notification Service.");
			result = Status.STATUS_CHANNEL_CLOSED;
			break;
		case 406:
			user.sendToastNotifications = false;
			dao.ofy().put(user);
			resp.getWriter().println("Push channel error: Web service has reached the per-day throttling limit for a subscription.");
			result = Status.STATUS_HOURLY_QUEUEABLE;
			break;
		case 412:
			user.sendToastNotifications = false;
			dao.ofy().put(user);
			resp.getWriter().println("Push channel error: The device is in an inactive state.");
			result = Status.STATUS_HOURLY_QUEUEABLE;
			break;
		case 503:
			resp.getWriter().println("Push channel error: The Push Notification Service is unable to process the request.");
			result = Status.STATUS_ERROR;
			break;
		default:
			resp.getWriter().println("Push channel error: Unknown error. HTTP status: "+status+", Notification status: "+NotificationStatus
					+", Device connection status: "+DeviceConnectionStatus+", Subsrciption status: "+SubscriptionStatus);
			result = Status.STATUS_ERROR;
		}	//TODO joku resend sopiviin kohtiin?
		resp.getWriter().close();
		return result;
	}
	private enum Status{
		STATUS_OK, STATUS_QUEUEABLE, STATUS_HOURLY_QUEUEABLE, STATUS_ERROR, STATUS_CHANNEL_CLOSED
	}
}
