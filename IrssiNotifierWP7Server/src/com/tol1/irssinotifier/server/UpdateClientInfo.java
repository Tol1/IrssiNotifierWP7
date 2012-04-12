package com.tol1.irssinotifier.server;

import java.io.IOException;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.google.appengine.api.datastore.Entity;
import com.google.appengine.api.datastore.EntityNotFoundException;
import com.google.appengine.api.datastore.Key;
import com.google.appengine.api.datastore.KeyFactory;

@SuppressWarnings("serial")
public class UpdateClientInfo extends HttpServlet {
	
	@Override
	protected void doPost(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		String id = req.getParameter("apiToken");
		String oldUrl = req.getParameter("oldUrl");
		String newUrl = req.getParameter("newUrl");
		
		if(id == null || oldUrl == null || newUrl == null){
			IrssiNotifier.printError(resp.getWriter(), "Virheellinen pyyntö");
			return;
		}
		
		Key userIdKey = KeyFactory.createKey("User", id);
		Entity databaseUser;
		try {
			databaseUser = IrssiNotifier.datastore.get(userIdKey);
		} catch (EntityNotFoundException e) {
			IrssiNotifier.printError(resp.getWriter(), "Virheellinen pyyntö");
			return;
		}
		if(databaseUser.getProperty("ChannelURI").equals(oldUrl)){
        	databaseUser.setProperty("ChannelURI", newUrl);
			IrssiNotifier.datastore.put(databaseUser);
			resp.getWriter().println("{ \"success\": true }");
			resp.getWriter().close();
			return;
		}
		IrssiNotifier.printError(resp.getWriter(), "Virheellinen pyyntö");
	}
}
