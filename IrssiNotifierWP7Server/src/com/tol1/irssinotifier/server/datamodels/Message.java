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
import com.googlecode.objectify.annotation.Indexed;
import com.googlecode.objectify.annotation.Parent;
import com.googlecode.objectify.annotation.Unindexed;
import com.tol1.irssinotifier.server.IrssiNotifier;
import com.tol1.irssinotifier.server.exceptions.XmlGeneratorException;

public class Message {
	@Id public Long id;
	@Indexed public Long timestamp;
	@Unindexed public String nick;
	@Unindexed public String channel;
	@Unindexed public String message;
	@Parent Key<IrssiNotifierUser> owner;
	
	public Message(){
		this.timestamp = System.currentTimeMillis();
		this.id = this.timestamp;
	}
	
	public Message(String nick, String channel, String message, IrssiNotifierUser user){
		this.timestamp = System.currentTimeMillis();
		this.id = this.timestamp;
		this.nick = nick;
		this.channel = channel;
		this.message = message;
		this.owner = new Key<IrssiNotifierUser>(user.getClass(), user.UserID);
	}
	
	public String GenerateToastNotification() throws XmlGeneratorException{
		
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
			param.appendChild(doc.createTextNode(IrssiNotifier.HILITEPAGEURL+"?NavigatedFrom=Toast"));
			
			StringWriter output = new StringWriter();

		    Transformer transformer = TransformerFactory.newInstance().newTransformer();
		    transformer.transform(new DOMSource(doc), new StreamResult(output));
		    
		    return output.toString();
			
		} catch (ParserConfigurationException e) {
			throw new XmlGeneratorException(e);
		} catch (TransformerConfigurationException e) {
			throw new XmlGeneratorException(e);
		} catch (TransformerFactoryConfigurationError e) {
			throw new XmlGeneratorException(e);
		} catch (TransformerException e) {
			throw new XmlGeneratorException(e);
		}
	}
	
	public String GenerateTileNotification(int countValue, String tileUrl) throws XmlGeneratorException{
		
		try {
			DocumentBuilderFactory docFactory = DocumentBuilderFactory.newInstance();
			DocumentBuilder docBuilder = docFactory.newDocumentBuilder();
			Document doc = docBuilder.newDocument();
			doc.setXmlStandalone(true);
			Element root = doc.createElement("wp:Notification");
			root.setAttribute("xmlns:wp", "WPNotification");
			doc.appendChild(root);
			Element tile = doc.createElement("wp:Tile");
			root.appendChild(tile);
			if(tileUrl != null){
				tile.setAttribute("Id", tileUrl);
			}
			Element bgImage = doc.createElement("wp:BackgroundImage");
			tile.appendChild(bgImage);
			Element count = doc.createElement("wp:Count");
			tile.appendChild(count);
			if(countValue == 0){
				count.setAttribute("Action", "Clear");
			}
			count.appendChild(doc.createTextNode(countValue+""));
			Element title = doc.createElement("wp:Title");
			tile.appendChild(title);
			
			StringWriter output = new StringWriter();

		    Transformer transformer = TransformerFactory.newInstance().newTransformer();
		    transformer.transform(new DOMSource(doc), new StreamResult(output));
		    
		    return output.toString();
			
		} catch (ParserConfigurationException e) {
			throw new XmlGeneratorException(e);
		} catch (TransformerConfigurationException e) {
			throw new XmlGeneratorException(e);
		} catch (TransformerFactoryConfigurationError e) {
			throw new XmlGeneratorException(e);
		} catch (TransformerException e) {
			throw new XmlGeneratorException(e);
		}
	}
}
