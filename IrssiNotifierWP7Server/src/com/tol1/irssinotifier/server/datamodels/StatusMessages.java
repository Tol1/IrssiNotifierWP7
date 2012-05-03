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
		public ErrorMessage(){
			this.success = false;
		}
		public ErrorMessage(String message){
			this.success = false;
			this.errorMessage = message;
		}
	}
	public static class ChannelStatusMessage extends StatusMessage{
		public boolean toastStatus;
		public boolean tileStatus;
		public ChannelStatusMessage(){
			super();
			this.toastStatus = false;
			this.tileStatus = false;
		}
		public ChannelStatusMessage(boolean toastEnabled, boolean tileEnabled){
			super();
			this.toastStatus = toastEnabled;
			this.tileStatus = tileEnabled;
		}
	}
	public static class MessageListResponse {
		public long currentTimestamp;
		public Message nextMessage;
		public boolean isNextFetch;
		public Query<Message> messages;
		public MessageListResponse(Query<Message> messages){
			this.messages = messages;
			this.currentTimestamp = System.currentTimeMillis();
		}
	}
}
