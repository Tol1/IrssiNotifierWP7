package com.tol1.irssinotifier.server;

import java.io.IOException;
import java.io.PrintWriter;

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
import com.google.appengine.api.users.User;
import com.google.appengine.api.users.UserService;
import com.google.appengine.api.users.UserServiceFactory;

@SuppressWarnings("serial")
public class RegisterPhoneClient extends HttpServlet {

	@Override
	protected void doPost(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		UserService userService = UserServiceFactory.getUserService();
        User user = userService.getCurrentUser();
        
        DatastoreService datastore = DatastoreServiceFactory.getDatastoreService();
        
        if (user != null) {
        	String id = user.getUserId();
        	
        	Key userIdKey = KeyFactory.createKey("User", id);
        	Entity databaseUser;
        	try {
				databaseUser = datastore.get(userIdKey);
			} catch (EntityNotFoundException e) {
				databaseUser = new Entity(userIdKey);
				databaseUser.setProperty("ChannelURI", null);
				datastore.put(databaseUser);
			}
        	
			resp.setContentType("application/json");
			String uri = req.getParameter("PushChannelURI");
			if(uri != null){
				databaseUser.setProperty("ChannelURI", uri);
				datastore.put(databaseUser);
				resp.getWriter().println("{ \"success\": \"true\" }");
			}
			else{
				resp.getWriter().println("{ \"success\": \"false\" }");
			}
        } else {
            resp.sendRedirect(userService.createLoginURL(req.getRequestURI()));
        }
	}

	@Override
	protected void doGet(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		// TODO Auto-generated method stub
		PrintWriter out = resp.getWriter();

	    out.println("<title>Example</title>" +
	       "<body bgcolor=FFFFFF>");
	    out.close();
	}
	
}
