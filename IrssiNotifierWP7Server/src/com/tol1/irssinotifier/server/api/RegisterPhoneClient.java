package com.tol1.irssinotifier.server.api;

import java.io.IOException;
import java.util.UUID;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.google.appengine.api.users.User;
import com.google.appengine.api.users.UserService;
import com.google.appengine.api.users.UserServiceFactory;
import com.tol1.irssinotifier.server.IrssiNotifier;
import com.tol1.irssinotifier.server.datamodels.IrssiNotifierUser;
import com.tol1.irssinotifier.server.datamodels.StatusMessages.RegisterSuccessMessage;
import com.tol1.irssinotifier.server.enums.TileType;
import com.tol1.irssinotifier.server.exceptions.OldVersionException;
import com.tol1.irssinotifier.server.utils.ObjectifyDAO;

import flexjson.JSONSerializer;

@SuppressWarnings("serial")
public class RegisterPhoneClient extends HttpServlet {
	
	private static UserService userService = UserServiceFactory.getUserService();
	
	@Override
	protected void doPost(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		resp.setCharacterEncoding("UTF-8");
		if(!IrssiNotifier.versionCheck(req.getParameter("version"))){
			IrssiNotifier.printError(resp.getWriter(), new OldVersionException());
			return;
		}
		ObjectifyDAO dao = new ObjectifyDAO();
		User user = userService.getCurrentUser();
        if (user != null) {
        	String guid = req.getParameter("guid");
			if(guid == null || guid.equals("")){
				IrssiNotifier.printError(resp.getWriter(), "GUID missing");
				return;
			}
			
			boolean wp8CompliantPhone = Boolean.parseBoolean(req.getParameter("wp8"));
			
        	String id = user.getUserId();
        	
        	UUID uuid = UUID.randomUUID();
        	
        	IrssiNotifierUser iNUser = new IrssiNotifierUser(id, guid, uuid.toString());
        	if(wp8CompliantPhone) {
        		iNUser.tileTemplate = TileType.WP8_FLIP;
        	}
        	else{
        		iNUser.tileTemplate = TileType.WP7;
        	}
        	
        	dao.ofy().put(iNUser);
        	RegisterSuccessMessage message = new RegisterSuccessMessage(uuid.toString());
        	resp.getWriter().println(new JSONSerializer().exclude("class").serialize(message));
			resp.getWriter().close();
			IrssiNotifier.log.info("Rekisteröitiin uusi käyttäjä "+id+" GUID:illa "+guid+" ja UUID:lla "+uuid);
			
        } else {
            resp.sendRedirect("/client/login");
        }
	}
}
