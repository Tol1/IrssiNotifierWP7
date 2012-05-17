package com.tol1.irssinotifier.server.datamodels;

import javax.persistence.*;

public class IrssiNotifierUser {
	@Id public String UserID;
	public String UUID;
	public String ChannelURI;
	public String guid;
	public boolean sendToastNotifications;
	public boolean sendTileNotifications;
	public boolean errorOccurred;
	public int tileCount;
	
	public IrssiNotifierUser(){}
	
	public IrssiNotifierUser(String id, String guid, String uuid){
		this.UserID = id;
		this.guid = guid;
		this.UUID = uuid;
	}
}
