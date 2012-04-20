package com.tol1.irssinotifier.server;
import java.io.PrintWriter;

import com.google.appengine.api.datastore.DatastoreService;
import com.google.appengine.api.datastore.DatastoreServiceFactory;
import com.google.appengine.api.datastore.Entity;
import com.google.appengine.api.datastore.EntityNotFoundException;
import com.google.appengine.api.datastore.Key;
import com.google.appengine.api.datastore.KeyFactory;
import com.google.appengine.api.users.UserService;
import com.google.appengine.api.users.UserServiceFactory;
import com.tol1.irssinotifier.server.datamodels.StatusMessages;

import flexjson.JSONSerializer;

public class IrssiNotifier {
	
	public static UserService userService = UserServiceFactory.getUserService();
	public static DatastoreService datastore = DatastoreServiceFactory.getDatastoreService();
	
	public static final String HILITEPAGEURL = "/Pages/HilitePage.xaml";
	
	public static void printError(PrintWriter writer, String errorMessage){
		StatusMessages.ErrorMessage message = new StatusMessages.ErrorMessage(errorMessage);
		String json = new JSONSerializer().exclude("class").serialize(message);
		writer.println(json);
		writer.close();
	}
	
	public static Entity checkAuthentication(String userId, String guid) throws Exception{
		if(userId == null || guid == null){
			throw new Exception("Virheellinen pyyntö");
		}
		Key userIdKey = KeyFactory.createKey("User", userId);
		Entity databaseUser;
		try {
			databaseUser = IrssiNotifier.datastore.get(userIdKey);
		} catch (EntityNotFoundException e) {
			throw new Exception("Virheellinen userid");
		}
		if(!databaseUser.getProperty("guid").equals(guid)){
			throw new Exception("Virheellinen guid");
		}
		return databaseUser;
	}
}
