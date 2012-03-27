package com.tol1.irssinotifier.server;
import java.io.IOException;
import java.io.PrintWriter;

import javax.servlet.ServletException;
import javax.servlet.http.*;

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
public class IrssiNotifierServlet extends HttpServlet {
	@Override
	protected void doPost(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		System.out.println(req.getParameterMap());
		resp.setContentType("text/html");
	    PrintWriter out = resp.getWriter();

	    out.println("<title>Example</title>" +
	       "<body bgcolor=FFFFFF>");

	    out.println("<h2>Button Clicked</h2>");

	    String DATA = req.getParameter("DATA");

	    if(DATA != null){
	      out.println(DATA);
	    } else {
	      out.println("No text entered.");
	    }

	    out.println("<P>Return to <A HREF=\"../simpleHTML.html\">Form</A>");
	    out.close();
	}

	public void doGet(HttpServletRequest req, HttpServletResponse resp) throws IOException {
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
        	
        	
            resp.setContentType("text/plain");
            resp.getWriter().println("Hello, " + user.getNickname());
        } else {
            resp.sendRedirect(userService.createLoginURL(req.getRequestURI()));
        }
	}
	
}
