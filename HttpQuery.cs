using netproxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


public static class HttpQuery
{
	static HttpQuery()
	{
		HttpQuery.Client.Timeout = TimeSpan.FromSeconds(10.0);
	}

	public static string Get(string url)
	{
		bool flag = false;
		for (; ; )
		{
			HttpQueryMode httpMode = ServerConsole.HttpMode;
			if (httpMode != HttpQueryMode.HttpRequest)
			{
				if (httpMode != HttpQueryMode.HttpClient)
				{
					break;
				}
			}
			else
			{
				try
				{
					WebRequest webRequest = WebRequest.Create(url);
					ServicePointManager.Expect100Continue = true;
					((HttpWebRequest)webRequest).UserAgent = "SCP SL";
					webRequest.Method = "GET";
					webRequest.ContentType = "application/x-www-form-urlencoded";
					using (WebResponse response = webRequest.GetResponse())
					{
						using (Stream responseStream = response.GetResponseStream())
						{
							using (StreamReader streamReader = new StreamReader(responseStream))
							{
								return streamReader.ReadToEnd();
							}
						}
					}
				}
				catch (Exception ex)
				{
					if (flag || ServerConsole.LockHttpMode || !ex.Message.Contains("(ReadDone1)"))
					{
						throw;
					}
					flag = true;
					ServerConsole.HttpMode = HttpQueryMode.HttpClient;
					Console.WriteLine("Switched to HttpClient (\"ReadDone1\" exception).");
					continue;
				}
			}
			try
			{
				Task<HttpResponseMessage> async = HttpQuery.Client.GetAsync(url);
				async.Wait();
				return async.Result.Content.ReadAsStringAsync().Result;
			}
			catch (Exception ex2)
			{
				if (flag || ServerConsole.LockHttpMode || !ex2.Message.Contains("One or more errors occurred"))
				{
					throw;
				}
				flag = true;
				ServerConsole.HttpMode = HttpQueryMode.HttpRequest;
				Console.WriteLine("Switched to HttpRequest (\"One or more errors...\" exception).");
				continue;
			}
		}
		return null;
	}

	public static string Post(string url, string data)
	{
		bool flag = false;
		for (; ; )
		{
			HttpQueryMode httpMode = ServerConsole.HttpMode;
			if (httpMode != HttpQueryMode.HttpRequest)
			{
				if (httpMode != HttpQueryMode.HttpClient)
				{
					break;
				}
			}
			else
			{
				try
				{
					byte[] bytes = new UTF8Encoding().GetBytes(data);
					WebRequest webRequest = WebRequest.Create(url);
					ServicePointManager.Expect100Continue = true;
					((HttpWebRequest)webRequest).UserAgent = "SCP SL";
					webRequest.Method = "POST";
					webRequest.ContentType = "application/x-www-form-urlencoded";
					webRequest.ContentLength = (long)bytes.Length;
					using (Stream requestStream = webRequest.GetRequestStream())
					{
						requestStream.Write(bytes, 0, bytes.Length);
					}
					using (WebResponse response = webRequest.GetResponse())
					{
						using (Stream responseStream = response.GetResponseStream())
						{
							using (StreamReader streamReader = new StreamReader(responseStream))
							{
								return streamReader.ReadToEnd();
							}
						}
					}
				}
				catch (Exception ex)
				{
					if (flag || ServerConsole.LockHttpMode || !ex.Message.Contains("(ReadDone1)"))
					{
						throw;
					}
					flag = true;
					ServerConsole.HttpMode = HttpQueryMode.HttpClient;
					Console.WriteLine("Switched to HttpClient (\"ReadDone1\" exception).");
					continue;
				}
			}
			try
			{
				Task<HttpResponseMessage> task = HttpQuery.Client.PostAsync(url, new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded"));
				task.Wait();
				return task.Result.Content.ReadAsStringAsync().Result;
			}
			catch (Exception ex2)
			{
				if (flag || ServerConsole.LockHttpMode || !ex2.Message.Contains("One or more errors occurred"))
				{
					throw;
				}
				flag = true;
				ServerConsole.HttpMode = HttpQueryMode.HttpRequest;
				Console.WriteLine("Switched to HttpRequest (\"One or more errors...\" exception).");
				continue;
			}
		}
		return null;
	}

	public static string ToPostArgs(IEnumerable<string> data)
	{
		return data.Aggregate((string current, string a) => current + "&" + a.Replace("&", "[AMP]")).TrimStart(new char[]
		{
			'&'
		});
	}

	public static readonly HttpClient Client = new HttpClient();
}