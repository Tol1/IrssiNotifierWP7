package com.tol1.irssinotifier.server;
import java.io.PrintWriter;
import java.util.logging.Logger;

import com.google.appengine.api.datastore.DatastoreService;
import com.google.appengine.api.datastore.DatastoreServiceFactory;
import com.google.appengine.api.users.User;
import com.google.appengine.api.users.UserService;
import com.google.appengine.api.users.UserServiceFactory;
import com.google.apphosting.api.ApiProxy;
import com.tol1.irssinotifier.server.datamodels.IrssiNotifierUser;
import com.tol1.irssinotifier.server.datamodels.StatusMessages;
import com.tol1.irssinotifier.server.exceptions.UserNotFoundException;
import com.tol1.irssinotifier.server.utils.ObjectifyDAO;

import flexjson.JSONSerializer;

public class IrssiNotifier {
	
	public static UserService userService = UserServiceFactory.getUserService();
	public static DatastoreService datastore = DatastoreServiceFactory.getDatastoreService();
	public static final Logger log = Logger.getLogger("com.tol1.irssinotifier.server");
	
	public static final String HILITEPAGEURL = "/Pages/HilitePage.xaml";
	
	public static void printErrorForIrssi(PrintWriter writer, String errorMessage){
		writer.print(errorMessage);
		writer.close();
		log.severe(errorMessage);
	}
	
	public static void printErrorForIrssi(PrintWriter writer, Exception exception){
		printErrorForIrssi(writer, exception.getLocalizedMessage());
	}
	
	public static void printError(PrintWriter writer, String errorMessage){
		StatusMessages.ErrorMessage message = new StatusMessages.ErrorMessage(errorMessage);
		writeError(writer, message);
	}
	
	public static void printError(PrintWriter writer, Exception exception){
		StatusMessages.ErrorMessage message = new StatusMessages.ErrorMessage(exception.getLocalizedMessage(), exception.getClass().getSimpleName());
		writeError(writer, message);
	}
	
	private static void writeError(PrintWriter writer, StatusMessages.ErrorMessage message){
		String json = new JSONSerializer().exclude("class").serialize(message);
		writer.print(json);
		writer.close();
		log.severe(message.errorMessage);
	}
	
	public static IrssiNotifierUser getUser(ObjectifyDAO dao, String uuid) throws UserNotFoundException{
		IrssiNotifierUser user = dao.ofy().query(IrssiNotifierUser.class).filter("UUID =", uuid.trim()).get();
		if(user == null){
			throw new UserNotFoundException();
		}
		return user;
	}
	
	public static IrssiNotifierUser getUser(ObjectifyDAO dao, User googleUser) throws UserNotFoundException{
		IrssiNotifierUser user = dao.ofy().get(IrssiNotifierUser.class, googleUser.getUserId());
		if(user == null){
			throw new UserNotFoundException();
		}
		return user;
	}
	
	public static int getCurrentVersion(){
		try {
			return Integer.parseInt(ApiProxy.getCurrentEnvironment().getVersionId().split("\\.")[0]);
		} catch (NumberFormatException e) {
			return 0;
		}
	}
	
	public static boolean versionCheck(String versionString){
		if(versionString == null){
			return false;
		}
		try{
			int serviceVersion = getCurrentVersion();
			int version = Integer.parseInt(versionString);
			return serviceVersion <= version;
		}
		catch(NumberFormatException nfe){
			return false;
		}
	}
}
