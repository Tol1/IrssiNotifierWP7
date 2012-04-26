package com.tol1.irssinotifier.server;

import java.io.IOException;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.googlecode.objectify.Query;
import com.tol1.irssinotifier.server.datamodels.*;
import com.tol1.irssinotifier.server.datamodels.StatusMessages.MessageListResponse;
import com.tol1.irssinotifier.server.exception.UserNotFoundException;
import com.tol1.irssinotifier.server.utils.CustomTimeTransformer;
import com.tol1.irssinotifier.server.utils.ObjectifyDAO;

import flexjson.JSONSerializer;

@SuppressWarnings("serial")
public class MessageList extends HttpServlet {

	@Override
	protected void doGet(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		listMessages(req, resp);
	}
	
	@Override
	protected void doPost(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		listMessages(req, resp);
	}
	private void listMessages(HttpServletRequest req, HttpServletResponse resp) throws ServletException, IOException{
		String id = req.getParameter("apiToken");
		String guid = req.getParameter("guid");
		resp.setCharacterEncoding("UTF-8");
		
		if(id == null || guid == null){
			IrssiNotifier.printError(resp.getWriter(), "Virheellinen pyyntö");
			return;
		}
		
		ObjectifyDAO dao = new ObjectifyDAO();
		try {
			IrssiNotifierUser user = IrssiNotifier.getUser(dao, id);
			Query<Message> messages = dao.ofy().query(Message.class).ancestor(user);
			String since = req.getParameter("since");
			if(since != null){
				try {
					long sinceTimestamp = Long.parseLong(since);
					messages = messages.filter("timestamp >", sinceTimestamp);
				} catch (NumberFormatException e) {}
			}
			MessageListResponse response = new MessageListResponse(messages);
			JSONSerializer serializer = new JSONSerializer();
			String jsonObject = serializer.include("messages").transform(new CustomTimeTransformer(), "messages.timestamp").exclude("*.class").serialize(response);
			resp.setHeader("Content-Type", "application/json");
			resp.getWriter().println(jsonObject);
			resp.getWriter().close();
		} catch (UserNotFoundException e) {
			IrssiNotifier.printError(resp.getWriter(), e.getLocalizedMessage());
		}
	}
}
