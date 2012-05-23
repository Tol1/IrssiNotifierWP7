package com.tol1.irssinotifier.server.login;

import java.io.IOException;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.google.appengine.api.users.User;
import com.google.appengine.api.users.UserService;
import com.google.appengine.api.users.UserServiceFactory;
import com.tol1.irssinotifier.server.IrssiNotifier;
import com.tol1.irssinotifier.server.exceptions.OldVersionException;

@SuppressWarnings("serial")
public class LoginUser extends HttpServlet {
	
	private static UserService userService = UserServiceFactory.getUserService();
	
	@Override
	protected void doGet(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		resp.setCharacterEncoding("UTF-8");
		if(!IrssiNotifier.versionCheck(req.getParameter("version"))){
			IrssiNotifier.printErrorForIrssi(resp.getWriter(), new OldVersionException());	//TODO sivu ympärille...
			return;
		}
		
		User user = userService.getCurrentUser();
		
		if (user != null) {
			resp.sendRedirect(req.getRequestURI()+"/loginsuccess");
		} else {
			resp.sendRedirect(userService.createLoginURL(req.getRequestURI()+"/loginsuccess"));
		}
	}
}
