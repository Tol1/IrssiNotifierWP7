package com.tol1.irssinotifier.server.login;

import java.io.IOException;

import javax.servlet.ServletException;
import javax.servlet.http.Cookie;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.google.appengine.api.users.User;
import com.google.appengine.api.users.UserService;
import com.google.appengine.api.users.UserServiceFactory;

@SuppressWarnings("serial")
public class LogoutUser extends HttpServlet {
	
	private static UserService userService = UserServiceFactory.getUserService();
	
	@Override
	protected void doGet(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		User user = userService.getCurrentUser();
		resp.setContentType("text/html");
		if(user != null){
			Cookie[] cookies = req.getCookies();
			for (Cookie cookie : cookies) {
				if(cookie.getName().equals("ACSID") || cookie.getName().equals("SACSID")){
					cookie.setMaxAge(0);
					resp.addCookie(cookie);
				}
			}
			resp.getWriter().println("Uloskirjautuminen onnistui");
		}
		else{
			resp.getWriter().println("Et ole kirjautuneena sisään");
		}
		resp.getWriter().close();
	}

}
