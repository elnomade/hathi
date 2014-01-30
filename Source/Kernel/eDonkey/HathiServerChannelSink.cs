#region Copyright (c) 2013 Hathi Project < http://hathi.sourceforge.net >
/*
* This file is part of Hathi Project
* Hathi Developers Team:
* andrewdev, beckman16, biskvit, elnomade_devel, ershyams, grefly, jpierce420,
* knocte, kshah05, manudenfer, palutz, ramone_hamilton, soudamini, writetogupta
*
* Hathi is a fork of Lphant Version 1.0 GPL
* Lphant Team
* Juanjo, 70n1, toertchn, FeuerFrei, mimontyf, finrold, jicxicmic, bladmorv,
* andrerib, arcange|, montagu, wins, RangO, FAV, roytam1, Jesse
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; either
* version 2 of the License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, write to the Free Software
* Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.IO;
using System.Net;
using System.Runtime.Remoting;
using Hathi.Types;
using Hathi.eDonkey.Utils;

namespace Hathi.eDonkey
{
	[Serializable]
	internal class HathiServerChannelSink : BaseChannelObjectWithProperties, IServerChannelSink
	{
		private IServerChannelSink m_NextIServerChannelSink = null;
		private HathiServerSinkProvider m_Provider = null;
		private IChannelReceiver m_channel = null;
		private CompressionType m_CompressionMethod = (CompressionType)
						Enum.Parse(typeof(CompressionType),
												(string)CKernel.Preferences.GetProperty("CompressionMethod"));

		#region Constructor

		public HathiServerChannelSink(IServerChannelSinkProvider Provider, IChannelReceiver channel)
		{
			IServerChannelSink nextServer = (IServerChannelSink)new BinaryServerFormatterSink(
																				BinaryServerFormatterSink.Protocol.Other, this.NextChannelSink, channel);
			if (channel != null) m_channel = channel;
			if (Provider != null) m_Provider = Provider as HathiServerSinkProvider;
			m_NextIServerChannelSink = new HathiServerChannelSink(Provider, channel, nextServer);
		}

		public HathiServerChannelSink(IServerChannelSinkProvider Provider, IChannelReceiver channel, object nextobject)
		{
			if (channel != null) m_channel = channel;
			if (Provider != null) m_Provider = Provider as HathiServerSinkProvider;
			if (nextobject != null)
			{
				m_NextIServerChannelSink = nextobject as IServerChannelSink;
				if (m_NextIServerChannelSink == null)
					m_NextIServerChannelSink = new BinaryServerFormatterSink(
							BinaryServerFormatterSink.Protocol.Other, this.NextChannelSink, channel);
			}
		}
		#endregion

		#region Miembros de IServerChannelSink

		public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack,
																		object state, IMessage msg, ITransportHeaders headers)
		{
			return this.m_NextIServerChannelSink.GetResponseStream(sinkStack, state, msg, headers);
		}

		public System.Runtime.Remoting.Channels.ServerProcessing ProcessMessage
		(IServerChannelSinkStack sinkStack, IMessage requestMsg,
		 ITransportHeaders requestHeaders, Stream requestStream,
		 out IMessage responseMsg, out ITransportHeaders responseHeaders,
		 out Stream responseStream)
		{
			//Iniciacion de variables, las respuestas no deben estar inicializadas, ver la documentacion
			//de ProcessMessage
			responseMsg = null;
			responseHeaders = null;
			responseStream = null;
			ServerProcessing processing = ServerProcessing.Complete;
			object state = null;
			//Obtener la ip remota a traves del canal actual y comprobar si esta permitida.
			//Si lo esta la procesamos y la enviamos a la pila de procesos (siguiente canal).
			IPAddress ipCliente = (IPAddress)requestHeaders[CommonTransportKeys.IPAddress];
			if (checkip(ipCliente.ToString()))
			{
				//Sustituci�n de requestStream que esta cerrado. Propiedad CanWrite no deja modificarlo.
				//no se puede ms=requestStream !!!
				//porque copia tambien las propiedades.
				//Tampoco directamente sobre el Stream, no se inicializan!!!
				if (!requestStream.CanWrite)
					AbrirStream(ref requestStream,
											System.Convert.ToInt32((string)requestHeaders["Tama�oComprimido"]));
				//Preprocesade del mensaje, descompresion
				ProcessRequest(requestMsg, requestHeaders, ref requestStream, ref state);
				//Envio del mensaje atraves del siguiente canal.
				//El siguiente IServerChannelSink es standar y no lo gestionamos.
				//vease IServerSinkProvider.CreateSink que es llamado automaticamente al crear el TcpServerChannel
				//Y los constructores de esta clase
				try
				{
					if ((bool)state)
						processing = this.NextChannelSink.ProcessMessage(
														 sinkStack, requestMsg, requestHeaders, requestStream,
														 out responseMsg, out responseHeaders, out responseStream);
				}
				catch (RemotingException e)
				{
					//Console.Write(e.Message);
					responseMsg = new ReturnMessage(e, (IMethodCallMessage)requestMsg);
				}
				//Comprobamos que responseStream esta abierto.
				if (!responseStream.CanWrite)
					AbrirStream(ref responseStream,
											System.Convert.ToInt32((string)responseMsg.Properties["Tama�oComprimido"]));
				//PostProcesado del mensaje, compresion de la respuesta.
				ProcessResponse(responseMsg, responseHeaders, ref responseStream, state);
				//Depuracion
				/*if (responseMsg.Properties["__MethodName"]==null)
						Console.WriteLine("Peticion desconocida");
				else
						Console.WriteLine("Peticion: " + (string)responseMsg.Properties["__MethodName"]);
				Console.WriteLine("Datos recibidos:" +
						(string)requestHeaders["Tama�o"] + "/" +
						(string)requestHeaders["Tama�oComprimido"]);
				Console.WriteLine("Datos enviados:" +
						System.Convert.ToString(responseHeaders["Tama�o"]) + "/" +
						System.Convert.ToString(responseHeaders["Tama�oComprimido"])   );
				*/
				//fin depuracion
			}
			else
			{
				Exception exp = new Exception(string.Format("{0}:IP nonallowed.", m_Provider.ToString()));
				responseMsg = new ReturnMessage(exp, (IMethodCallMessage)requestMsg);
			}
			return processing;
		}

		public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack,
																		 object state, IMessage msg, ITransportHeaders headers, Stream stream)
		{
			//Como no se suele ejecutar no he podido ver las cabeceras ni el mensaje
			if (!stream.CanWrite)
			{
				try
				{
					AbrirStream(ref stream);
				}
				catch
				{
					AbrirStream(ref stream,
											System.Convert.ToInt32((string)msg.Properties["Tama�oComprimido"]));
				}
			}
			ProcessResponse(msg, headers, ref stream, state);
			try
			{
				sinkStack.AsyncProcessResponse(msg, headers, stream);
			}
			catch (RemotingException e)
			{
				Console.Write(e.Message);
			}
		}

		public IServerChannelSink NextChannelSink
		{
			get
			{
				return m_NextIServerChannelSink;
			}
		}

		#endregion

		#region procesado del mensaje, compresion / descompresion
		private void ProcessRequest(IMessage message, ITransportHeaders headers, ref Stream stream, ref object state)
		{
			state = true;
			if ((string)headers["edonkeyCompress"] == "Yes")
			{
				//Descomprimir  y quitar la cabecera
				CompressionType c = CompressionType.Zip;
				int com = System.Convert.ToInt32(headers["CompressionType"]);
				if (Enum.GetName(typeof(CompressionType), com) != null)
				{
					c = (CompressionType)com;
					Descompresion descompresor = new Descompresion(stream, c);
					Stream descomprimido = descompresor.ToStream;
					if (descomprimido != null)
					{
						headers["CompressionType"] = null;
						headers["edonkeyCompress"] = null;
						//Si descomentamos no podemos hacer la depuracion
						//headers["Tama�oComprimido"] = null;
						//headers["Tama�o"] = null;
						stream = descomprimido;
					}
					else
					{
						stream = null;
						Exception exp = new Exception(string.Format("{0}:Error, could not be decompressed.", m_Provider.ToString()));
						IMessage responseMsg = new ReturnMessage(exp, (IMethodCallMessage)message);
						System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf =
								new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
						bf.Serialize(stream, responseMsg);
					}
				}
				else
				{
					stream = null;
					Exception exp = new Exception(string.Format("{0}:Error, unknown method of compression.", m_Provider.ToString()));
					IMessage responseMsg = new ReturnMessage(exp, (IMethodCallMessage)message);
					System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf =
							new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
					bf.Serialize(stream, responseMsg);
				}
			}
		}

		private void ProcessResponse(IMessage message, ITransportHeaders headers, ref Stream stream, object state)
		{
			if (state != null)
			{
				if (headers != null)
				{
					//Comprimir y marcar la cabecera.
					Compresion compresor = new Compresion(stream, m_CompressionMethod);
					Stream comprimido = compresor.ToStream;
					if (comprimido != null)
					{
						if (comprimido.Length < stream.Length)
						{
							headers["edonkeyCompress"] = "Yes";
							headers["Tama�oComprimido"] = comprimido.Length;
							headers["Tama�o"] = stream.Length;
							headers["CompressionType"] = (int)compresor.CompressionProvider;
							stream = comprimido;
						}
					}
				}
			}
		}

		#endregion

		#region Otras Funciones
		private void AbrirStream(ref Stream stream)
		{
			AbrirStream(ref stream, (int)stream.Length);
		}
		private void AbrirStream(ref Stream stream, int tama�o)
		{
			MemoryStream ms = new MemoryStream(tama�o);
			byte[] Data = new byte[tama�o];
			stream.Read(Data, 0, tama�o);
			ms.Write(Data, 0, tama�o);
			stream = null;
			stream = ms;
		}
		private bool checkip(string IP)
		{
			string[] IPAllowed = (string[])CKernel.Preferences.GetProperty("AllowedIP");
			for (int i = 0; i < IPAllowed.Length; i++)
			{
				if (IPAllowed[i] == IP) return true;
				if (IPAllowed[i] == "255.255.255.255") return true;
			}
			return false;
		}
		#endregion

	}

}
