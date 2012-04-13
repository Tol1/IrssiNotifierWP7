package com.tol1.irssinotifier.server;

import java.io.IOException;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.google.appengine.api.datastore.Entity;

@SuppressWarnings("serial")
public class UpdateSettings extends HttpServlet {
	@Override
	protected void doPost(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		String id = req.getParameter("apiToken");
		String guid = req.getParameter("guid");
		
		Entity databaseUser;
		try {
			databaseUser = IrssiNotifier.checkAuthentication(id, guid);
		} catch (Exception e) {
			IrssiNotifier.printError(resp.getWriter(), e.getLocalizedMessage());
			return;
		}
		String param;
		if((param = req.getParameter("enable")) != null){
			databaseUser.setProperty("sendToastNotifications", Boolean.parseBoolean(param));
		}
		IrssiNotifier.datastore.put(databaseUser);
		resp.getWriter().println("{ \"success\": true }");
		resp.getWriter().close();
		return;
	}
}
