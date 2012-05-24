package com.tol1.irssinotifier.server.datamodels;

import com.googlecode.objectify.Query;

public class StatusMessages {
	public static class StatusMessage{
		public boolean success;
		public StatusMessage(){
			this.success = true;
		}
	}
	
	public static class RegisterSuccessMessage extends StatusMessage{
		public String userid;
		public RegisterSuccessMessage(String userid){
			this.success = true;
			this.userid = userid;
		}
	}
	
	public static class ErrorMessage extends StatusMessage{
		public String errorMessage;
		public String exceptionType;
		public ErrorMessage(){
			this.success = false;
		}
		public ErrorMessage(String message){
			this.success = false;
			this.errorMessage = message;
		}
		public ErrorMessage(String message, String exceptionType){
			this.success = false;
			this.errorMessage = message;
			this.exceptionType = exceptionType;
		}
	}
	public static class ChannelStatusMessage extends StatusMessage{
		public boolean toastStatus;
		public boolean tileStatus;
		public int toastOffset;
		public boolean errorStatus;
		public ChannelStatusMessage(){
			super();
			this.toastStatus = false;
			this.tileStatus = false;
			this.errorStatus = false;
			this.toastOffset = 15;
		}
		public ChannelStatusMessage(boolean toastEnabled, boolean tileEnabled, boolean errorStatus, int toastOffset){
			super();
			this.toastStatus = toastEnabled;
			this.tileStatus = tileEnabled;
			this.toastOffset = toastOffset;
			this.errorStatus = errorStatus;
		}
	}
	public static class MessageListResponse extends StatusMessage {
		public long currentTimestamp;
		public Message nextMessage;
		public boolean isNextFetch;
		public Query<Message> messages;
		public MessageListResponse(Query<Message> messages){
			super();
			this.messages = messages;
			this.currentTimestamp = System.currentTimeMillis();
		}
	}
}
