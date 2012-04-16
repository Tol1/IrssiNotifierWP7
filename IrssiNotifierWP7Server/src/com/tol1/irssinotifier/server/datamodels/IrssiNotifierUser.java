package com.tol1.irssinotifier.server.datamodels;

import javax.persistence.*;

public class IrssiNotifierUser {
	@Id public String UserID;
	public String ChannelURI;
	public String guid;
	public boolean sendToastNotifications;
	public boolean sendTileNotifications;
	
	public IrssiNotifierUser(){}
	
	public IrssiNotifierUser(String id, String guid){
		this.UserID = id;
		this.guid = guid;
	}
	
/*	databaseUser.setProperty("ChannelURI", null);
	databaseUser.setProperty("guid", null);
	databaseUser.setProperty("sendToastNotifications", false);
	databaseUser.setProperty("sendTileNotifications", false);*/
}
