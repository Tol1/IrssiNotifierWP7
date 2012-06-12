package com.tol1.irssinotifier.server.exceptions;

@SuppressWarnings("serial")
public class XmlGeneratorException extends Exception {

	public XmlGeneratorException() {
		super("Error generating XML for push channel");
	}

	public XmlGeneratorException(String message) {
		super(message);
	}

	public XmlGeneratorException(Throwable cause) {
		super(cause);
	}

	public XmlGeneratorException(String message, Throwable cause) {
		super(message, cause);
	}

	public XmlGeneratorException(String message, Throwable cause,
			boolean enableSuppression, boolean writableStackTrace) {
		super(message, cause, enableSuppression, writableStackTrace);
	}

}
