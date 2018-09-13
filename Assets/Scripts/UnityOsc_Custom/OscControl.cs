//
//	  UnityOSC - Example of usage for OSC receiver
//
//	  Copyright (c) 2012 Jorge Garcia Martin
//	  Last edit: Gerard Llorach 2nd August 2017
//
// 	  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// 	  documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// 	  the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
// 	  and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// 	  The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// 	  of the Software.
//
// 	  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// 	  TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// 	  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// 	  CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// 	  IN THE SOFTWARE.
//

using UnityEngine;
using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityOSC;

public class OscControl : MonoBehaviour {

	public enum OscMode { SEND, RECEIVE, SEND_RECEIVE };
	public OscMode oscMode = OscMode.RECEIVE;
    public string outIP = "127.0.0.1";
    public int outPort = 9999;
    public int inPort = 9998;

	[HideInInspector] public int maxDots = 100;
	[HideInInspector] public Vector2[] dot1, dot2;
	[HideInInspector] public List<string> hostList;
	[HideInInspector] public int numHosts = 2;

	private OSCServer myServer;
	private int bufferSize = 100; // Buffer size of the application (stores 100 messages from different servers)
	private int rxBufferSize = 1024;
	private int sleepMs = 10;


    // Script initialization
    void Start() {
		dot1 = new Vector2[maxDots];
		dot2 = new Vector2[maxDots];
		hostList = new List<string>();

		// init OSC
        OSCHandler.Instance.Init(); 

        // Initialize OSC clients (transmitters)
		if (oscMode == OscMode.SEND || oscMode == OscMode.SEND_RECEIVE) {
			OSCHandler.Instance.CreateClient("myClient", IPAddress.Parse(outIP), outPort);
		}
        
		if (oscMode == OscMode.RECEIVE || oscMode == OscMode.SEND_RECEIVE) {
			// Initialize OSC servers (listeners)
			myServer = OSCHandler.Instance.CreateServer("myServer", inPort);
			// Set buffer size (bytes) of the server (default 1024)
			myServer.ReceiveBufferSize = rxBufferSize;
			// Set the sleeping time of the thread (default 10)
			myServer.SleepMilliseconds = sleepMs;
		}
    }

	// Reads all the messages received between the previous update and this one
	void Update() {
		if (oscMode == OscMode.RECEIVE || oscMode == OscMode.SEND_RECEIVE) {
			// Read received messages
			for (var i = 0; i < OSCHandler.Instance.packets.Count; i++) {
				// Process OSC
				receivedOSC(OSCHandler.Instance.packets[i]);
				// Remove them once they have been read.
				OSCHandler.Instance.packets.Remove(OSCHandler.Instance.packets[i]);
				i--;
			}
		}

        // Send random number to the client
		if (oscMode == OscMode.SEND || oscMode == OscMode.SEND_RECEIVE) {
			float randVal = UnityEngine.Random.Range(0f, 0.7f);
			OSCHandler.Instance.SendMessageToClient("myClient", "/1/fader1", randVal);
		}
    }

    // Process OSC message
    private void receivedOSC(OSCPacket pckt) {
        if (pckt == null) { 
			Debug.Log("Empty packet"); 
			return; 
		}

		string hostName = (string) pckt.Data[0];
		int index = (int) pckt.Data[1];
		float x = (float) pckt.Data[2];
		float y = (float) pckt.Data[3];
		float z = 0f;

		if (hostList.Count >= numHosts) {
			if (hostName == hostList[0]) {
				dot1[index] = new Vector2(x, y);
			} else {
				dot2[index] = new Vector2(x, y);
			}
		} else {
			hostList.Add(hostName);
		}

		/*
		switch ((string) pckt.Data[0]) {
			case (name1):
				pos1 = new Vector3(x, -y, z);
				break;
			case (name2):
				pos2 = new Vector3(x, -y, z);
				break;
		}
		*/

        /*
        // Origin
        int serverPort = pckt.server.ServerPort;

        // Address
        string address = pckt.Address.Substring(1);

        // Data at index 0
        string data0 = pckt.Data.Count != 0 ? pckt.Data[0].ToString() : "null";

        // Print out messages
        Debug.Log("Input port: " + serverPort.ToString() + "\nAddress: " + address + "\nData [0]: " + data0);
		*/
    }

}