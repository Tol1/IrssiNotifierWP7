package com.tol1.irssinotifier.server;

import java.io.IOException;
import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.google.appengine.api.users.User;
import com.google.appengine.api.users.UserService;
import com.google.appengine.api.users.UserServiceFactory;
import com.tol1.irssinotifier.server.datamodels.IrssiNotifierUser;
import com.tol1.irssinotifier.server.datamodels.StatusMessages.RegisterSuccessMessage;
import com.tol1.irssinotifier.server.utils.ObjectifyDAO;

import flexjson.JSONSerializer;

@SuppressWarnings("serial")
public class RegisterPhoneClient extends HttpServlet {
	
	private static UserService userService = UserServiceFactory.getUserService();
	
	@Override
	protected void doPost(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		ObjectifyDAO dao = new ObjectifyDAO();
		User user = userService.getCurrentUser();
        if (user != null) {
        	String guid = req.getParameter("guid");
			if(guid == null){
				IrssiNotifier.printError(resp.getWriter(), "GUID missing");
				return;
			}
        	String id = user.getUserId();
        	
        	IrssiNotifierUser iNUser = new IrssiNotifierUser(id, guid);
        	
        	dao.ofy().put(iNUser);
        	RegisterSuccessMessage message = new RegisterSuccessMessage(id);
        	resp.getWriter().println(new JSONSerializer().exclude("class").serialize(message));
			resp.getWriter().close();
			
        } else {
            resp.sendRedirect("/client/login");
        }
	}
}
