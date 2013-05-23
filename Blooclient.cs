using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace ki113d.CsBloocoin {

    /// <summary>
    /// Object representation of a deserialized server command reply.
    /// </summary>
    /// <typeparam name="T">Command reply payload value type. Used for special deserialization cases.</typeparam>
    public class CmdReply<T> {
        public Boolean success = false;
        public String message = String.Empty;
        public Dictionary<String, T> payload = new Dictionary<String, T>();
        public String toString() {
            return String.Format("Success={0}; Message={1}; Payload={2}; ",
                success, message, payload.ToString());
        }
    }

    /// <summary>
    /// Object representation of a Bloocoin transaction.
    /// </summary>
    public class Transaction {
        public String to = String.Empty;
        public String from = String.Empty;
        public int amount = 0;
    }

    /// <summary>
    /// Handles all Bloocoin client server interaction.
    /// </summary>
    public class BloocoinClient {

        private String ip;
        private int port;
        private String address;
        private String pwd;
        private String bloocoinFolder;

        /// <summary>
        /// Dictionary mapping of commands the server handles.
        /// </summary>
        public Dictionary<String, String[]> commands =
            new Dictionary<String, String[]>() {
            // Return: {{"difficulty": difficulty}}
            {"get_coin",     new String[] {
            }},
            // Returns: {{"addr": addr}}
            {"register",     new String[] {
                "addr", "pwd",
            }},
            // Returns: {{"to": to}, {"from": from}, {"amount": amount}}
            {"send_coin",    new String[] {
                "to", "addr", "pwd", "amount",
            }},
            // Returns: {{"amount": amount}}
            {"my_coins",     new String[] {
                "addr", "pwd",
            }},
            // Returns: {{"hash": hash}}
            {"check",        new String[] {
                "winning_string", "winning_hash", "addr",
            }},
            {"transactions", new String[] {
                "addr", "pwd",
            }},
            // Returns: {{"amount": amount}}
            {"total_coins",  new String[] {
            }},
            // Returns: {{"addr": addr}, {"amount": amount}}
            {"check_addr",   new String[] {
                "addr",
            }},
        };
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ip">Server IP address or hostname.</param>
        /// <param name="port">The port on which the server listens for client connections.</param>
        public BloocoinClient( String ip = "server.bloocoin.org", int port = 3122 ) {
            this.ip = ip;
            this.port = port;
            
            bloocoinFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            bloocoinFolder += @"\.bloocoin\";
        }

        // Client util functions

        /// <summary>
        /// Getter for the client instances address.
        /// </summary>
        /// <returns>The address for this instance.</returns>
        public String getAddress() {
            return address;
        }

        /// <summary>
        /// Getter for the client instances pwd.
        /// </summary>
        /// <returns>The pwd for this instance.</returns>
        public String getPwd() {
            return pwd;
        }

        /// <summary>
        /// Getter for the directory path of the .bloocoin folder
        /// </summary>
        /// <returns>.bloocoin directory path.</returns>
        public String getBloocoinDir() {
            return bloocoinFolder;
        }

        /// <summary>
        /// Generates a new Bloostamp.
        /// </summary>
        /// <returns>String representation of the address and key separated by a colon.</returns>
        public String generateBloostamp() {
            return String.Format("{0}:{1}", Util.sha1().hash, Util.sha1().hash);
        }

        /// <summary>
        /// Sets the address and password of the Bloocoin client instance.
        /// </summary>
        /// <param name="address">The address from the users Bloostamp.</param>
        /// <param name="pwd">The password from the users Bloostamp.</param>
        public void setAddrAndPwd( String address, String pwd ) {
            this.address = address;
            this.pwd = pwd;
        }

        // Client server interaction

        /// <summary>
        /// Proxy function for getReply/>.
        /// </summary>
        /// <typeparam name="T">Command reply payload value type.</typeparam>
        /// <param name="command">String representing the command name to be executed.</param>
        /// <param name="address">Optional address parameter.</param>
        /// <param name="pwd">Optional password parameter.</param>
        /// <returns>Custom CmdReply object.</returns>
        public CmdReply<T> getReply<T>(String command, String address = null, String pwd = null) {
            return getReply<T>(command, new Dictionary<String, Object>(), address, pwd);
        }

        /// <summary>
        /// Sends a command to the server and returns a custom CmdReply object.
        /// </summary>
        /// <typeparam name="T">Command reply payload value type.</typeparam>
        /// <param name="command">String representing the command name to be executed.</param>
        /// <param name="args">Any method parameters needed for commands.</param>
        /// <param name="address">Optional address parameter.</param>
        /// <param name="pwd">Optional password parameter.</param>
        /// <returns>Custom CmdReply object.</returns>
        public CmdReply<T> getReply<T>( String command, Dictionary<String, Object> args,
            String address = null, String pwd = null) {

            if (!commands.ContainsKey(command))
                throw new Exceptions.InvalidArgumentException("Invalid command provided!");

            Dictionary<String, Object> temp = new Dictionary<String, Object>();
            temp.Add("cmd", command);
            
            address = (address == null) ? this.address : address;
            pwd = (pwd == null) ? this.pwd : pwd;

            foreach (String arg in commands[command]) {
                if (arg == "addr")
                    temp.Add("addr", address);
                else if (arg == "pwd")
                    temp.Add("pwd", pwd);
                else {
                    if (!args.ContainsKey(arg))
                        throw new Exceptions.InvalidArgumentException("Invalid command argument provided.");
                    temp.Add(arg, args[arg]);
                }
            }
            
            CmdReply<T> cmd = new CmdReply<T>();
            String re = String.Empty;
            Console.WriteLine("Command: {0}", temp.ToString());
            using (SocketHandler sHandler = new SocketHandler(ip, port)) {
                if (sHandler.connect()) {
                    sHandler.send(JsonConvert.SerializeObject(temp));
                    while (true) {
                        String str = String.Empty;
                        str = sHandler.receive();
                        if (str != String.Empty)
                            re += str;
                        try {
                            cmd = JsonConvert.DeserializeObject<CmdReply<T>>(re);
                            break;
                        } catch (Exception ohShit) {
                            // Do nothing... received data is incomplete...
                        }
                    }
                }
            }
            return cmd;
        }

        /// <summary>
        /// Simplified function for registering a Bloostamp.
        /// </summary>
        /// <param name="addr">The new address to be registered.</param>
        /// <param name="pwd">The new password to be registered.</param>
        /// <returns>The success of the register operation.</returns>
        public Boolean register( String addr, String pwd ) {
            return getReply<String>("register", addr, pwd).success;
        }

        /// <summary>
        /// Check is a connection to the server is successful.
        /// </summary>
        /// <returns>Returns whether the connection to the server is successful.</returns>
        public Boolean isOnline() {
            using (SocketHandler sHandler = new SocketHandler(ip, port)) {
                return sHandler.connect();
            }
        }

        // File operations

        /// <summary>
        /// Checks if a Bloostamp exists.
        /// </summary>
        /// <returns>Returns whether the Bloostamp exists.</returns>
        public Boolean bloostampExists() {
            return File.Exists(bloocoinFolder + "bloostamp");
        }

        /// <summary>
        /// Proxy function.
        /// Writes a new Bloostamp to file.
        /// </summary>
        /// <param name="address">The new Address.</param>
        /// <param name="pwd">The new Password.</param>
        public void writeBloostamp( String address, String pwd ) {
            writeBloostamp(String.Format("{0}:{1}", address, pwd));
        }

        /// <summary>
        /// Writes a new Bloostamp to file.
        /// </summary>
        /// <param name="stamp">The stamp to be written to file.</param>
        public void writeBloostamp( String stamp ) {
            File.WriteAllText(bloocoinFolder + "bloostamp", stamp);
        }

        /// <summary>
        /// Reads the Bloostamp from file.
        /// </summary>
        /// <returns>String representation of the Bloostamp.</returns>
        public String readBloostamp() {
            return File.ReadAllText(bloocoinFolder + "bloostamp");
        }
    }
}
