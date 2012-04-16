package com.tol1.irssinotifier.server.datamodels;

import java.io.StringWriter;

import javax.persistence.Id;
import javax.xml.parsers.*;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerConfigurationException;
import javax.xml.transform.TransformerException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.TransformerFactoryConfigurationError;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;

import org.w3c.dom.Document;
import org.w3c.dom.Element;

import com.googlecode.objectify.Key;
import com.googlecode.objectify.annotation.Parent;

public class Message {
	@Id public Long Timestamp;
	public String nick;
	public String channel;
	public String message;
	@Parent Key<IrssiNotifierUser> owner;
	
	public Message(){
		this.Timestamp = System.currentTimeMillis();
	}
	
	public Message(String nick, String channel, String message, IrssiNotifierUser user){
		this.Timestamp = System.currentTimeMillis();
		this.nick = nick;
		this.channel = channel;
		this.message = message;
		this.owner = new Key<IrssiNotifierUser>(user.getClass(), user.UserID);
	}
	
	public String GenerateToastNotification(){
		
		try {
			DocumentBuilderFactory docFactory = DocumentBuilderFactory.newInstance();
			DocumentBuilder docBuilder = docFactory.newDocumentBuilder();
			Document doc = docBuilder.newDocument();
			doc.setXmlStandalone(true);
			Element root = doc.createElement("wp:Notification");
			root.setAttribute("xmlns:wp", "WPNotification");
			doc.appendChild(root);
			Element toast = doc.createElement("wp:Toast");
			root.appendChild(toast);
			Element text1 = doc.createElement("wp:Text1");
			toast.appendChild(text1);
			text1.appendChild(doc.createTextNode(this.nick + " @ " + this.channel));
			Element text2 = doc.createElement("wp:Text2");
			toast.appendChild(text2);
			text2.appendChild(doc.createTextNode(this.message));
			Element param = doc.createElement("wp:Param");
			toast.appendChild(param);
			param.appendChild(doc.createTextNode("/Page2.xaml?NavigatedFrom=Toast Notification"));
			
			StringWriter output = new StringWriter();

		    Transformer transformer = TransformerFactory.newInstance().newTransformer();
		    transformer.transform(new DOMSource(doc), new StreamResult(output));
		    
		    return output.toString();
			
		} catch (ParserConfigurationException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (TransformerConfigurationException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (TransformerFactoryConfigurationError e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (TransformerException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} 
		
		String toast = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
				+ "<wp:Notification xmlns:wp=\"WPNotification\">"
					+ "<wp:Toast>"
						+ "<wp:Text1>"
							+ this.nick + " - " + this.channel
						+ "</wp:Text1>"
						+ "<wp:Text2>"
							+ this.message
						+ "</wp:Text2>"
						+ "<wp:Param>/Page2.xaml?NavigatedFrom=Toast Notification</wp:Param>"
				+ "</wp:Toast> "
			+ "</wp:Notification>";
		return toast;
	}
}
