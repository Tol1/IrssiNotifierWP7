package com.tol1.irssinotifier.server.utils;

import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.Date;

import flexjson.transformer.AbstractTransformer;

public class CustomTimeTransformer extends AbstractTransformer {

	@Override
	public void transform(Object arg0) {
		if(arg0 instanceof Long){
			Long timestamp = (Long)arg0;
			DateFormat df = new SimpleDateFormat("yyyy-MM-dd'T'HH:mmZ");
			this.getContext().writeQuoted(df.format(new Date(timestamp)));
		}
	}

}
