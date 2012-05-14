package com.tol1.irssinotifier.server.exceptions;

@SuppressWarnings("serial")
public class InvalidGUIDException extends Exception {

	public InvalidGUIDException() {
		super("GUID ei täsmää");
	}

	public InvalidGUIDException(String message) {
		super(message);
	}

}
