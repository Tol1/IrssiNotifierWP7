package com.tol1.irssinotifier.server;
import java.io.PrintWriter;
import java.util.Map;

import com.google.appengine.api.datastore.DatastoreService;
import com.google.appengine.api.datastore.DatastoreServiceFactory;
import com.google.appengine.api.users.UserService;
import com.google.appengine.api.users.UserServiceFactory;

public class IrssiNotifier {
	
	public static UserService userService = UserServiceFactory.getUserService();
	public static DatastoreService datastore = DatastoreServiceFactory.getDatastoreService();
	
	public static void printError(PrintWriter writer, String errorMessage){
		writer.println(errorMessage);
		writer.close();
	}
	
	/*public static boolean checkAuthentication(Map<String, String> parameters){
		String userId = parameters.get("apiToken");
		String oldUrl = parameters.get("oldUrl");
		return userId != null && oldUrl != null;
	}*/
}
