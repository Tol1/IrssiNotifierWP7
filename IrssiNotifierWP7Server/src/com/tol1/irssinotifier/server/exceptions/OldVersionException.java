package com.tol1.irssinotifier.server.exceptions;

@SuppressWarnings("serial")
public class OldVersionException extends Exception {

	public OldVersionException() {
		super("Käytössäsi on sovelluksen vanha versio. Päivitä sovellus jatkaaksesi.");
	}

	public OldVersionException(String message) {
		super(message);
	}
}
