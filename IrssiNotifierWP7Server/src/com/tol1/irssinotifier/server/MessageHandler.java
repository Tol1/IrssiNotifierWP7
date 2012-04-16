package com.tol1.irssinotifier.server;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.net.HttpURLConnection;
import java.net.URL;
import java.net.URLConnection;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.tol1.irssinotifier.server.datamodels.IrssiNotifierUser;
import com.tol1.irssinotifier.server.datamodels.Message;

@SuppressWarnings("serial")
public class MessageHandler extends HttpServlet {

	@Override
	protected void doPost(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		String id = req.getParameter("apiToken");
		
		ObjectifyDAO dao = new ObjectifyDAO();
		
		IrssiNotifierUser user = dao.ofy().get(IrssiNotifierUser.class, id);
		
		URL url = new URL(user.ChannelURI);
		
		/*
		Entity databaseUser;
		try {
			databaseUser = datastore.get(userIdKey);
		} catch (EntityNotFoundException e) {
			resp.getWriter().println("Virheellinen pyyntö");
			resp.getWriter().close();
			return;
		}
		URL url = new URL((String) databaseUser.getProperty("ChannelURI"));*/
		
		Message mess = new Message(req.getParameter("nick"), req.getParameter("channel"), req.getParameter("message"),user);
		
		if(user.sendToastNotifications){
			String toastMessage = mess.GenerateToastNotification();
			HttpURLConnection conn = (HttpURLConnection)url.openConnection();
			conn.setRequestMethod("POST");
			conn.setDoOutput(true);
			conn.setUseCaches(false);
			conn.setRequestProperty("Content-Type", "text/xml");
			conn.setRequestProperty("X-WindowsPhone-Target", "toast");
			conn.setRequestProperty("X-NotificationClass", "2");
	
			OutputStreamWriter writer = null;
			try {
				writer = new OutputStreamWriter(conn.getOutputStream(), "UTF-8");
				writer.write(toastMessage); // Write POST query string (if any
											// needed).
			} finally {
				if (writer != null)
					try {
						writer.close();
					} catch (IOException logOrIgnore) {
					}
			}
			
			int status = conn.getResponseCode();
			String NotificationStatus = conn.getHeaderField("X-NotificationStatus");
			String DeviceConnectionStatus = conn.getHeaderField("X-DeviceConnectionStatus");
			String SubscriptionStatus = conn.getHeaderField("X-SubscriptionStatus");
			switch(status){
			case 200:
				if(NotificationStatus.equalsIgnoreCase("Received")){
					return;
				}
				else if(NotificationStatus.equalsIgnoreCase("QueueFull")){
					resp.getWriter().println("Push channel error: Queue overflow.");
				}
				else{
					resp.getWriter().println("Push channel error: Suppressed.");
				}
				break;
			case 400:
				resp.getWriter().println("Push channel error: Bad XML.");
				break;
			case 404:
				user.sendToastNotifications = false;
				dao.ofy().put(user);
				resp.getWriter().println("Push channel error: The subscription is invalid and is not present on the Push Notification Service.");
				break;
			case 406:
				user.sendToastNotifications = false;
				dao.ofy().put(user);
				resp.getWriter().println("Push channel error: Web service has reached the per-day throttling limit for a subscription.");
				break;
			case 412:
				user.sendToastNotifications = false;
				dao.ofy().put(user);
				resp.getWriter().println("Push channel error: The device is in an inactive state.");
				break;
			case 503:
				resp.getWriter().println("Push channel error: The Push Notification Service is unable to process the request.");
				break;
			}	//TODO joku resend sopiviin kohtiin?
			resp.getWriter().close();
			/*if(status == 200){
				
			}
	
			InputStream result = conn.getInputStream();
	
			BufferedReader br = new BufferedReader(new InputStreamReader(result));
	
			String moi = "", errorMessage = "";
			while (null != (moi = br.readLine())) {
				errorMessage += moi;
			}
			if(errorMessage.length() > 0){
				resp.getWriter().println("Push channel error");
			}
			br.close();
			resp.getWriter().close();*/
		}
	}
}
