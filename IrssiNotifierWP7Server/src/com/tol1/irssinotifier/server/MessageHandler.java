package com.tol1.irssinotifier.server;

import java.io.BufferedReader;
import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.net.URI;
import java.net.URISyntaxException;
import java.net.URL;
import java.net.URLConnection;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.google.appengine.api.datastore.DatastoreService;
import com.google.appengine.api.datastore.DatastoreServiceFactory;
import com.google.appengine.api.datastore.Entity;
import com.google.appengine.api.datastore.EntityNotFoundException;
import com.google.appengine.api.datastore.Key;
import com.google.appengine.api.datastore.KeyFactory;
import com.google.appengine.api.users.UserService;
import com.google.appengine.api.users.UserServiceFactory;

public class MessageHandler extends HttpServlet {

	private static UserService userService = UserServiceFactory.getUserService();
	private static DatastoreService datastore = DatastoreServiceFactory.getDatastoreService();
	
	@Override
	protected void doGet(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		String id = req.getParameter("apiToken");
		Key userIdKey = KeyFactory.createKey("User", id);
    	Entity databaseUser;
    	try {
			databaseUser = datastore.get(userIdKey);
		} catch (EntityNotFoundException e) {
			resp.getWriter().println("Virheellinen pyyntö");
			resp.getWriter().close();
			return;
		}
    	URL url = new URL((String) databaseUser.getProperty("ChannelURI"));
		String nick = req.getParameter("nick");
		String channel = req.getParameter("channel");
		String message = req.getParameter("message");
		String toastMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
		        "<wp:Notification xmlns:wp=\"WPNotification\">" +
		           "<wp:Toast>" +
		                "<wp:Text1>" + nick +" - "+ channel + "</wp:Text1>" +
		                "<wp:Text2>" + message + "</wp:Text2>" +
		                "<wp:Param>/Page2.xaml?NavigatedFrom=Toast Notification</wp:Param>" +
		           "</wp:Toast> " +
		        "</wp:Notification>";
		byte[] tavut = toastMessage.getBytes("UTF-8");
		URLConnection conn = url.openConnection();
		conn.setDoOutput(true);
		conn.setUseCaches(false);
		conn.setRequestProperty("Content-Type", "text/xml");
		conn.setRequestProperty("X-WindowsPhone-Target", "toast");
		conn.setRequestProperty("X-NotificationClass", "2");
		
		OutputStreamWriter writer = null;
		try {
		    writer = new OutputStreamWriter(conn.getOutputStream(), "UTF-8");
		    writer.write(toastMessage); // Write POST query string (if any needed).
		} finally {
		    if (writer != null) try { writer.close(); } catch (IOException logOrIgnore) {}
		}

		InputStream result = conn.getInputStream();
		
		BufferedReader br = new BufferedReader(new InputStreamReader(result));
		
		String moi = "";
		while(null != (moi = br.readLine())){
			resp.getWriter().println(moi);
		}
		br.close();
		resp.getWriter().close();
		/*
		DataOutputStream printout = new DataOutputStream (conn.getOutputStream ());
		printout.writeBytes(toastMessage);
		printout.flush ();
	    printout.close ();
	    
	    DataInputStream input = new DataInputStream (conn.getInputStream ());
	    char str;
	    try {
			while((str = input.readChar()) != 0)
			{
				resp.getWriter().println(str);
			}
		} catch (Exception e) {
			resp.getWriter().println(e.toString());
		}
	    resp.getWriter().close();
	    input.close ();*/
	}

	@Override
	protected void doPost(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		String id = req.getParameter("apiToken");
		Key userIdKey = KeyFactory.createKey("User", id);
    	Entity databaseUser;
    	try {
			databaseUser = datastore.get(userIdKey);
		} catch (EntityNotFoundException e) {
			resp.getWriter().println("Virheellinen pyyntö");
			resp.getWriter().close();
			return;
		}
    	URL url = new URL((String) databaseUser.getProperty("ChannelURI"));
		String nick = req.getParameter("nick");
		String channel = req.getParameter("channel");
		String message = req.getParameter("message");
		String toastMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
		        "<wp:Notification xmlns:wp=\"WPNotification\">" +
		           "<wp:Toast>" +
		                "<wp:Text1>" + nick +" - "+ channel + "</wp:Text1>" +
		                "<wp:Text2>" + message + "</wp:Text2>" +
		                "<wp:Param>/Page2.xaml?NavigatedFrom=Toast Notification</wp:Param>" +
		           "</wp:Toast> " +
		        "</wp:Notification>";
		URLConnection conn = url.openConnection();
		conn.setDoOutput(true);
		conn.setUseCaches(false);
		conn.setRequestProperty("Content-Type", "text/xml");
		conn.setRequestProperty("X-WindowsPhone-Target", "toast");
		conn.setRequestProperty("X-NotificationClass", "2");
		
		OutputStreamWriter writer = null;
		try {
		    writer = new OutputStreamWriter(conn.getOutputStream(), "UTF-8");
		    writer.write(toastMessage); // Write POST query string (if any needed).
		} finally {
		    if (writer != null) try { writer.close(); } catch (IOException logOrIgnore) {}
		}

		InputStream result = conn.getInputStream();
		
		BufferedReader br = new BufferedReader(new InputStreamReader(result));
		
		String moi = "";
		while(null != (moi = br.readLine())){
			resp.getWriter().println(moi);
		}
		br.close();
		resp.getWriter().close();
	}
	
}
