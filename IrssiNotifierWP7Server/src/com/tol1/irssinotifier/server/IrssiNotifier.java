package com.tol1.irssinotifier.server;
import java.io.PrintWriter;
import java.util.logging.Logger;

import com.google.appengine.api.datastore.DatastoreService;
import com.google.appengine.api.datastore.DatastoreServiceFactory;
import com.google.appengine.api.users.UserService;
import com.google.appengine.api.users.UserServiceFactory;
import com.tol1.irssinotifier.server.datamodels.StatusMessages;

import flexjson.JSONSerializer;

public class IrssiNotifier {
	
	public static UserService userService = UserServiceFactory.getUserService();
	public static DatastoreService datastore = DatastoreServiceFactory.getDatastoreService();
	public static final Logger log = Logger.getLogger("com.tol1.irssinotifier.server");
	
	public static final String HILITEPAGEURL = "/Pages/HilitePage.xaml";
	
	public static void printError(PrintWriter writer, String errorMessage){
		StatusMessages.ErrorMessage message = new StatusMessages.ErrorMessage(errorMessage);
		String json = new JSONSerializer().exclude("class").serialize(message);
		writer.println(json);
		writer.close();
		log.severe(errorMessage);
	}
}
