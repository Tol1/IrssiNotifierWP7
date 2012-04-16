package com.tol1.irssinotifier.server;

import com.googlecode.objectify.ObjectifyService;
import com.googlecode.objectify.util.DAOBase;
import com.tol1.irssinotifier.server.datamodels.IrssiNotifierUser;

public class ObjectifyDAO extends DAOBase {
	static {
        ObjectifyService.register(IrssiNotifierUser.class);
    }

}
