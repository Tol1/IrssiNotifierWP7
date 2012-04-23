package com.tol1.irssinotifier.server;

import java.io.IOException;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.google.appengine.api.datastore.QueryResultIterable;
import com.googlecode.objectify.Key;
import com.tol1.irssinotifier.server.datamodels.Message;
import com.tol1.irssinotifier.server.utils.ObjectifyDAO;

@SuppressWarnings("serial")
public class Clear extends HttpServlet {

	@Override
	protected void doGet(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		ObjectifyDAO dao = new ObjectifyDAO();
		QueryResultIterable<Key<Message>> messages = dao.ofy().query(Message.class).filter("timestamp <", System.currentTimeMillis()-5*24*60*60*1000).fetchKeys();
		dao.ofy().delete(messages);
	}

}
