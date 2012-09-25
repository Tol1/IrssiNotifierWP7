package com.tol1.irssinotifier.server.exceptions;

@SuppressWarnings("serial")
public class UserNotFoundException extends Exception {

	public UserNotFoundException() {
		super("User was not found");
	}

	public UserNotFoundException(String userId) {
		super("User with token "+userId+" was not found");
	}
}
