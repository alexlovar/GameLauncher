using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;

namespace rlgn
{
    class Login : IDisposable
    {
        private string uname = "";
        private string pwd = "";
        private string error = "";
        private SHA1CryptoServiceProvider SHAprovider = new SHA1CryptoServiceProvider();
        private Socket lsock;
        private byte status;
        private uint serial;
        private uint serial_end;
        private ushort account_type;
        private string[] zoneServers;
        private byte[] zoneServerStates;
        private string TIP;

        public Login(string username, string password, string IP)
        {
            this.uname = username;
            this.pwd = password;
            this.uname.Trim();
            this.pwd.Trim();
            this.TIP = IP;
            //this.pwd = password;
        }

        public void Dispose()
        {
            try
            {
                DisposeSocket();
                SHAprovider.Dispose();
            }
            catch { }
        }

        public string Username
        {
            get
            {
                return uname;
            }
        }

        public int getZones() 
        {
            int ret = 0;
            int br = 0;
            if (sanitize())
            {
                lsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    //lsock.Connect("217.16.113.81", 10001);
                    //lsock.Connect("173.236.26.228", 10001);
                    lsock.Connect(TIP, 10001);
                    
                }
                catch (Exception e)
                {
                    ret = -2;
                    error += e.Message + "\n";
                    return ret;
                }
                byte[] buffer = new byte[] { 0x05, 0x00, 0x15, 0x0C, 0xFF };
                lsock.Send(buffer);
                buffer = new byte[1000];
                br = lsock.Receive(buffer);
                if (br < 7)
                {
                    ret = -3;
                    error += "Error during login: malformed data returned.\n";
                }
                else
                {
                    byte[] rsend = new byte[31]; //31 or 58
                    new byte[] { 0x1f, 0x00, 0x15, 0x03 }.CopyTo(rsend, 0);
                    try
                    {
                        encrypt(Encoding.ASCII.GetBytes(uname), new byte[] { buffer[5], buffer[6] }, buffer[4], 13).CopyTo(rsend, 4);
                        encrypt(Encoding.ASCII.GetBytes(pwd), new byte[] { buffer[5], buffer[6] }, buffer[4], 13).CopyTo(rsend, 17); // 13 oder 40
                    }
                    catch (Exception ex)
                    {
                        ret = -24;
                        error += "Error during encryption of u p." + ex.Message + "\n";
                    }
                    new byte[] { 0x00 }.CopyTo(rsend, 30); // 30 or 57
                    lsock.Send(rsend);

                    byte[] login_packet = new byte[100];
                    int lpcount = lsock.Receive(login_packet);
                    if (lpcount < 54)
                    {
                        ret = -4;
                        error += "Error during login: malformed data returned.\n";
                    }
                    else
                    {
                        status = login_packet[4];
                        serial = BitConverter.ToUInt32(login_packet, 5);
                        serial_end = serial;
                        account_type = BitConverter.ToUInt16(login_packet, 10);
                        serial ^= 0x6e65e0af;
                        serial_end ^= 0xc89c183a;
                        account_type ^= 0x4b3a;
                        if (status != 0x00)
                        {
                            ret = -5;
                            error += "Server status error:\n" + srv_errors(status);
                        }
                        else
                        {
                            lsock.Send(new byte[] { 0x08, 0x00, 0x15, 0x05, 0x00, 0x00, 0x00, 0x00 });
                            byte[] list_p = new byte[1023];
                            int listc = lsock.Receive(list_p);

                            /* ok, here we should figure out the server codes. what have we got so far?
                             * Wireshark: 11:00:15:06:00:0a:00:01:01:06:4b:75:64:6f:5a:00:00:07:00:15:42:01:00:00:06:00:15:43:01:00
                             * Translate: 17    21  6    10     1  1  6  K  u  d  o  Z        7    21 66  1        6    21 67  1   
                             * Translate2:msgtp:headr:lngth:  0  :  1  :  2  :  3  :  4  :  5  :  6  :  7  :  8  :  9  :footr:footr
                             * 
                             * 2 Zones
                             * 
                             *  Wireshark 1b 00 15 06 00 14 00 02
                             *  Translate 27  0 21  6    20  0  2 
                             *  01 06 4b 75 64 6f 5a 00 00 
                             *   1  6  K  u  d  o  Z      
                             *  01 07 4e 75 72 65 61 4c 00 00
                             *   1  7 N  u  r  e  a  L   0  0 
                             *  09 00 15 42 02 00 00 00 00
                             *   9  0 21 66  2  0  0  0  0  
                             *  07 00 15 43 02 00 00
                             *   7  0 21 67  2  0  0
                             *  Interpret 
                             * * */


                            if (listc < 28)
                            {
                                ret = -6;
                                error += "Receiving server list: malformed data returned.\n";
                            }
                            else
                            {
                                // make a list of Zone servers
                                try
                                {
                                    int isrv = (int)list_p[7];
                                    zoneServers = new string[isrv];
                                    zoneServerStates = new byte[isrv];
                                    int offset = 10;
                                    int length = (int)list_p[9] - 1;
                                    for (int i = 0; i < isrv; i++)
                                    {
                                        zoneServers[i] = ASCIIEncoding.ASCII.GetString(list_p, offset, length);
                                        zoneServerStates[i] = list_p[offset - 2];
                                        offset += length + 4;
                                        length = (int)list_p[offset - 1] - 1;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ret = -22;
                                    error += "Server list: Zone server packet malformed.\n" + ex.Message + "\n";
                                    return ret;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                ret = -1;
            }
            return ret;
        }

        public int doLogin(byte ZoneID)
        {
            int ret = 0;
            lsock.Send(new byte[] { 0x06, 0x00, 0x15, 0x07, ZoneID, 0x00 });
            // _select_world_request_cllo
            //                      m  s  g  h  e  a  d  r, word world#
            byte[] server_p = new byte[1023];
            int servc = lsock.Receive(server_p);
            if (servc < 28)
            {
                ret = -7;
                error += "Receiving server info: malformed data returned.\n";
            }
            else
            {
                uint server_ip;
                ushort server_port;
                uint[] master_keys;
                try
                {
                    server_ip = BitConverter.ToUInt32(server_p, 5);

                    server_port = BitConverter.ToUInt16(server_p, 9);
                    master_keys = new uint[] { BitConverter.ToUInt32(server_p, 11) ^ 0xcfcf22e6, BitConverter.ToUInt32(server_p, 15) ^ 0x5bbcde6f, BitConverter.ToUInt32(server_p, 19) ^ 0xacdf5eda, BitConverter.ToUInt32(server_p, 23) ^ 0xbccd1b37 };

                }
                catch (Exception ex)
                {
                    ret = -23;
                    error += "Server list: Zone server packet malformed.\n" + ex.Message + "\n";
                    return ret;
                }
                server_ip ^= 0xcb9c4b3a;
                server_port ^= 0x4fb6;

                ushort lang_id = 3 ^ 0x32d7;
                uint nullbytes = 0;
                uint adult = 0x1a003000 ^ 0xd29c283b;
                FileStream fh;
                string path;
                try
                {
                    path = System.AppDomain.CurrentDomain.BaseDirectory;
                    fh = new FileStream(path + "\\DefaultSet.tmp", FileMode.Create);
                }
                catch (Exception e)
                {
                    ret = -8;
                    error += e.Message;
                    return ret;
                }
                try
                {
                    byte[] buname = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    byte[] tmp_buname = Encoding.ASCII.GetBytes(uname);
                    for (int i = 0; i < tmp_buname.Length; i++)
                    {
                        buname[i] = tmp_buname[i];
                    }
                    fh.Write(BitConverter.GetBytes(server_ip), 0, 4);
                    fh.Write(BitConverter.GetBytes(server_port), 0, 2);
                    fh.Write(buname, 0, 13);
                    fh.Write(BitConverter.GetBytes(serial), 0, 4);
                    fh.Write(BitConverter.GetBytes(master_keys[0]), 0, 4);
                    fh.Write(BitConverter.GetBytes(master_keys[1]), 0, 4);
                    fh.Write(BitConverter.GetBytes(master_keys[2]), 0, 4);
                    fh.Write(BitConverter.GetBytes(master_keys[3]), 0, 4);
                    fh.Write(BitConverter.GetBytes(account_type), 0, 2);
                    fh.Write(BitConverter.GetBytes(serial_end), 0, 4);
                    fh.Write(BitConverter.GetBytes(adult), 0, 4);
                    fh.Write(BitConverter.GetBytes(nullbytes), 0, 4);
                    fh.Write(BitConverter.GetBytes(lang_id), 0, 2);
                    fh.Close();
                    
                    File.Delete(path + "\\System\\DefaultSet.tmp");
                    File.Copy(path + "\\DefaultSet.tmp", path + "\\System\\DefaultSet.tmp");
                }
                catch (Exception e)
                {
                    ret = -9;
                    error += e.Message;
                }
                finally
                {
                    fh.Dispose();
                }
            }
            return ret;
        }
        public string[] getServerList() 
        {
            return zoneServers;
        }

        public byte[] getServerStates() 
        {
            return zoneServerStates;
        }

        public string getLastError()
        {
            string ret = error;
            error = "";
            return ret;
        }

        public void DisposeSocket()
        {
            lsock.Disconnect(false);
            lsock.Close();
            lsock.Dispose();
        }

        private bool sanitize()
        {
            bool ret = true;

            if (uname.Length < 1)
            {
                error += "Username cannot be empty.\n";
                ret &= false;
            }
            if (pwd.Length < 1)
            {
                error += "Password cannot be empty.\n";
                ret &= false;
            }
            if (uname.Length > 24)
            {
                error += "Username longer than allowed.\n";
                ret &= false;
            }
            if (pwd.Length > 24)
            {
                error += "Password longer than allowed.\n";
                ret &= false;
            }

            Match umatch = Regex.Match(uname, "^[a-zA-Z0-9!][a-zA-Z0-9]*$");

            if (!umatch.Success)
            {
                error += "Username contains invalid characters";
                ret &= false;
            }

            Match pmatch = Regex.Match(pwd, "^[a-zA-Z0-9]*$");
            //Match pmatch = Regex.Match(pwd, "^[A-F0-9]*$");

            //System.Windows.Forms.MessageBox.Show(pwd, "Password hash", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);

            if (!pmatch.Success)
            {
                error += "Password contains invalid characters";
                ret &= false;
            }

            return ret;
        }

        private byte[] encrypt (byte[] str, byte[] key, byte plus, int num)
        {
            try
            {
                byte[] ret = new byte[num];
                int i = 0;
                for (i = 0; i < str.Length; i++)
                {
                    ret[i] = str[i];
                }
                i++;
                for (int j = i; j < num; j++)
                {
                    ret[j] = 0x00;
                }

                ushort skey = BitConverter.ToUInt16(key, 0);
                skey += 3;
                plus += 1;

                byte bkey = Convert.ToByte(skey % 256);

                for (int k = 0; k < num; k++)
                {
                    ret[k] = (byte)((byte)(ret[k] + plus) ^ bkey);
                }

                return ret;
            }
            catch
            {
                return null;
            }
        }

        private string srv_errors(int errno)
        {
            string ret = "Unknown error " + errno.ToString();

            switch (errno)
            {
                case 0:
                    ret = "Success."; break;
                case 1:
                    ret = "ASYNC problems."; break;
                case 2:
                    ret = "World closed."; break;
                case 5:
                    ret = "Already logged in."; break;
                case 6:
                    ret = "Invalid username."; break;
                case 7:
                    ret = "Invalid password."; break;
                case 24:
                    ret = "Your account has been banned."; break;
                case 28:
                    ret = "Password too long. Limit to 12 characters."; break;
                case 40:
                    ret = "Account banned."; break;
                case 48:
                    ret = "Login server closed. Ask Admin to /open."; break;
            }
            return ret;
        }
    }
}
