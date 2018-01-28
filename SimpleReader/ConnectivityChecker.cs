using System;
using System.Net;
using System.IO;

namespace SimpleReader {
    public static class ConnectivityChecker {
        public enum ConnectionStatus {
            NotConnected,
            LimitedAccess,
            Connected
        }

        public static ConnectionStatus CheckInternet() {
            // Проверить подключение к dns.msftncsi.com
            try {
                IPHostEntry entry = Dns.GetHostEntry("dns.msftncsi.com");
                if (entry.AddressList.Length == 0) {
                    return ConnectionStatus.NotConnected;
                }
                else {
                    if (!entry.AddressList[0].ToString().Equals("131.107.255.255")) {
                        return ConnectionStatus.LimitedAccess;
                    }
                }
            }
            catch {
                return ConnectionStatus.NotConnected;
            }

            // Проверить загрузку документа ncsi.txt
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://www.msftncsi.com/ncsi.txt");
            try {
                HttpWebResponse responce = (HttpWebResponse)request.GetResponse();

                if (responce.StatusCode != HttpStatusCode.OK) {
                    return ConnectionStatus.LimitedAccess;
                }
                using (StreamReader sr = new StreamReader(responce.GetResponseStream())) {
                    if (sr.ReadToEnd().Equals("Microsoft NCSI")) {
                        return ConnectionStatus.Connected;
                    }
                    else {
                        return ConnectionStatus.LimitedAccess;
                    }
                }
            }
            catch {
                return ConnectionStatus.NotConnected;
            }

        }
    }
}
