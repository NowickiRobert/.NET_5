using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace cs_lab5
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Task<long> taskDown;
        private Task<long> taskUp;
        BackgroundWorker fibBackgroundWorker;
        public MainWindow()
        {
            InitializeComponent();
           
        }

        private void fibBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            fibProgress.Value = e.ProgressPercentage;
        }

        //asynchronicznie 1-C
        private Task<long> AsyncNewton(Tuple<int, int> newt)
        {
            long k_fract = 1;
            long up_fract = newt.Item1;
            Task<long> r = Task.Factory.StartNew<long>(
                (obj) =>
                {
                    Tuple<int, int> t = (Tuple<int,int>) (obj);
                    for (int i = 1; i <= t.Item2; i++) k_fract *= i;
                    for (int i = 1; i < t.Item2; i++) up_fract *= up_fract - i;
                    long result = up_fract / k_fract;
                    return result;
                },
                newt
            );
            return r;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (k_input.Text == "" || n_input.Text == "")
            {
                asyncResult.Text = "Blad!";
                return;
            }
            int k = int.Parse(k_input.Text);
            int n = int.Parse(n_input.Text);
            Tuple<int, int> newt = new Tuple<int, int>(n, k);
            long result = await AsyncNewton(newt);
            asyncResult.Text = result > 0 ? result.ToString() : "Blad!";
        }

        //taski 1-A
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (k_input.Text == "" || n_input.Text == "")
            {
                taskResult.Text = "Blad!";
                return;
            }
            int k = int.Parse(k_input.Text);
            int n = int.Parse(n_input.Text);
            
            taskDown = Task.Factory.StartNew<long>(
                (obj) =>
                {
                    int max = (int) obj;
                    long res = 1;
                    for (int i = 1; i <= max; i++) res *= i;
                    return res;
                },
                k
            );
            //Console.WriteLine(taskD.Result);
            taskUp = Task.Factory.StartNew<long>(
                (obj) =>
                {
                    int max = (int)obj;
                    long res = max;
                    for (int i = 1; i <k; i++) res = res*(res-i);
                    return res;
                },
                n
            );
            long result = taskUp.Result / taskDown.Result;
            taskResult.Text = result > 0 ? result.ToString() : "Blad!";
        }

        public static long Newotn(int n, int k)
        {
            long k_fract = 1;
            long up_fract = n;
            for (int i = 1; i <= k; i++) k_fract *= i;
            for (int i = 1; i < k; i++) up_fract *= up_fract - i;
            return up_fract / k_fract;
        }


        //delegaty 1-B
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (k_input.Text == "" || n_input.Text == "")
            {
                delegatesResult.Text = "Blad!";
                return;
            }
            int k = int.Parse(k_input.Text);
            int n = int.Parse(n_input.Text);
            Func<int, int, long> op = Newotn;
            IAsyncResult result;
            result = op.BeginInvoke(n, k, null, null);
            while (result.IsCompleted == false)
            {
                Console.Write(".");
            }
            long res = op.EndInvoke(result);
            delegatesResult.Text = res > 0 ? res.ToString() : "Blad!";
        }

        //fibonacci
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (!fibBackgroundWorker.IsBusy)
            {
                fibBackgroundWorker = new BackgroundWorker();
                fibBackgroundWorker.WorkerReportsProgress = true;
                fibBackgroundWorker.DoWork += new DoWorkEventHandler(Do_work);
                fibBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(fibBackgroundWorker_ProgressChanged);
                fibBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_Completed);
                fibBackgroundWorker.RunWorkerAsync(fibInput.Text);
            }
        }

        private void worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled)
            {
                fibResult.Text = "Blad!";
                return;
            }
            fibResult.Text = (string)e.Result;
        }

        private void Do_work(object sender, DoWorkEventArgs e)
        {
            string s = (string)e.Argument;
            
            if (s == "" || int.Parse(s) < 1)
            {
                e.Result = "Blad!";
               
            }
            int n = int.Parse(s);
            if (n == 1 || n == 2) fibResult.Text = "1";
            int fib = 1;
            int fib_prev1 = 1;
            int fib_prev2 = 1;
            int progress;
            for (int i = 0; i < n; i++)
            {

                fib = fib_prev1 + fib_prev2;
                fib_prev2 = fib_prev1;
                fib_prev1 = fib;
                progress = (int) (Math.Ceiling((((i+1.0) / n) * 100)));
                fibBackgroundWorker.ReportProgress(progress);
                System.Threading.Thread.Sleep(20);               
            }
            e.Result = fib.ToString();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.ShowDialog();
            string path = folderBrowserDialog.SelectedPath;
            if (path == "") return;
            DirectoryInfo directorySelected = new DirectoryInfo(path);
            Compress(directorySelected, path);
        }

        private void Compress(DirectoryInfo directorySelected, string path)
        {
            foreach (FileInfo fileToCompress in directorySelected.GetFiles())
            {
                using (FileStream originalFileStream = fileToCompress.OpenRead())
                {
                    if ((File.GetAttributes(fileToCompress.FullName) &
                         FileAttributes.Hidden) != FileAttributes.Hidden & fileToCompress.Extension != ".gz")
                    {
                        using (FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz"))
                        {
                            using (GZipStream compressionStream = new GZipStream(compressedFileStream,
                                CompressionMode.Compress))
                            {
                                originalFileStream.CopyTo(compressionStream);

                            }
                        }
                        FileInfo info = new FileInfo(path + "\\" + fileToCompress.Name + ".gz");
                        Console.WriteLine("Compressed {0} from {1} to {2} bytes.",
                            fileToCompress.Name, fileToCompress.Length.ToString(), info.Length.ToString());
                    }

                }
            }
        }

        public static void Decompress(FileInfo fileToDecompress)
        {
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);
                using (FileStream decompressedFileStream = File.Create(newFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                        Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
                    }
                }
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.ShowDialog();
            string path = folderBrowserDialog.SelectedPath;
            if (path == "") return;
            DirectoryInfo directorySelected = new DirectoryInfo(path);
            foreach (FileInfo fileToDecompress in directorySelected.GetFiles("*.gz"))
            {
                Decompress(fileToDecompress);
            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            int port = 8080;
            string ip = "127.0.0.1";
            TcpClient client = new TcpClient(ip,port);
            string[] hostNames = { "www.microsoft.com", "www.apple.com",
                "www.google.com", "www.ibm.com", "cisco.netacad.net",
                "www.oracle.com", "www.nokia.com", "www.hp.com", "www.dell.com",
                "www.samsung.com", "www.toshiba.com", "www.siemens.com",
                "www.amazon.com", "www.sony.com", "www.canon.com", "www.alcatellucent.com",
                "www.acer.com", "www.motorola.com" };
            ;
            IPAddress[] ipAddresses;
            var permissionState = System.Security.Permissions.PermissionState.Unrestricted;
            DnsPermission dnsPermission = new DnsPermission(permissionState);
            dnsPermission.PermitOnly();
            for (var index = 0; index < hostNames.Length; index++)
            {
                string hN = hostNames[index];
                ipAddresses = Dns.GetHostAddresses(hN);
                ipAddress.Text +=hN+" => \n" + ipAddresses.ToString()+"\n";
            }
        }
    }
}
