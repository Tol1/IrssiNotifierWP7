package com.tol1.irssinotifier.server.api;

import java.io.IOException;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.tol1.irssinotifier.server.IrssiNotifier;
import com.tol1.irssinotifier.server.datamodels.IrssiNotifierUser;
import com.tol1.irssinotifier.server.datamodels.StatusMessages.StatusMessage;
import com.tol1.irssinotifier.server.exceptions.*;
import com.tol1.irssinotifier.server.utils.ObjectifyDAO;

import flexjson.JSONSerializer;

@SuppressWarnings("serial")
public class UpdateSettings extends HttpServlet {
	@Override
	protected void doPost(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		resp.setCharacterEncoding("UTF-8");
		if(!IrssiNotifier.versionCheck(req.getParameter("version"))){
			IrssiNotifier.printError(resp.getWriter(), new OldVersionException());
			return;
		}
		String id = req.getParameter("apiToken");
		String guid = req.getParameter("guid");
		
		if(id == null && guid == null || id.equals("") || guid.equals("")){
			IrssiNotifier.printError(resp.getWriter(), "Virheellinen pyyntö");
			return;
		}
		
		ObjectifyDAO dao = new ObjectifyDAO();
		try {
			IrssiNotifierUser user = IrssiNotifier.getUser(dao, id);
			
			if(user.guid.equals(guid)){
				String param;
				if((param = req.getParameter("toast")) != null){
					user.sendToastNotifications = Boolean.parseBoolean(param);
					IrssiNotifier.log.info("Käyttäjän "+id+" toast notificationien lähetys asetettu arvoon "+Boolean.parseBoolean(param));
				}
				if((param = req.getParameter("tile")) != null){
					user.sendTileNotifications = Boolean.parseBoolean(param);
					IrssiNotifier.log.info("Käyttäjän "+id+" tile notificationien lähetys asetettu arvoon "+Boolean.parseBoolean(param));
				}
				if((param = req.getParameter("clearcount")) != null){
					user.tileCount = 0;
					IrssiNotifier.log.info("Käyttäjän "+id+" tile count nollattu");
				}
				if((param = req.getParameter("toastoffset")) != null){
					user.toastNotificationOffset = Integer.parseInt(param);
					IrssiNotifier.log.info("Käyttäjän "+id+" toast notificationien lähetykselle asetettu "+Boolean.parseBoolean(param)+" sekunnin offset");
				}
				dao.ofy().put(user);
				resp.getWriter().println(new JSONSerializer().exclude("class").serialize(new StatusMessage()));
				resp.getWriter().close();
				return;
			}
			else{
				IrssiNotifier.printError(resp.getWriter(), new InvalidGUIDException());
			}
		} catch (UserNotFoundException e) {
			IrssiNotifier.printError(resp.getWriter(), e);
			return;
		}
	}
}
