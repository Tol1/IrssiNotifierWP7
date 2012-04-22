package com.tol1.irssinotifier.server.utils;

import com.googlecode.objectify.ObjectifyService;
import com.googlecode.objectify.util.DAOBase;
import com.tol1.irssinotifier.server.datamodels.IrssiNotifierUser;
import com.tol1.irssinotifier.server.datamodels.Message;

public class ObjectifyDAO extends DAOBase {
	static {
        ObjectifyService.register(IrssiNotifierUser.class);
        ObjectifyService.register(Message.class);
    }

}
