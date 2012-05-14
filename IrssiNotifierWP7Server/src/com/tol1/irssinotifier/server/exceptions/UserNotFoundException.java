package com.tol1.irssinotifier.server.exceptions;

@SuppressWarnings("serial")
public class UserNotFoundException extends Exception {

	public UserNotFoundException() {
		super("Käyttäjää ei löytynyt");
	}

	public UserNotFoundException(String message) {
		super(message);
	}
}
