using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace Produire.Network.SSH
{
	/// <summary></summary>
	[要求動作(RequireActions.NetworkAccess)]
	[種類(DocUrl = "html/network/ftpsimpleclient.htm")]
	public class SFTPクライアント : IProduireClass
	{
		#region フィールド

		SftpClient client = null;

		string host;
		int port = 22;
		string username;
		string password;
		string privateKeyFile;

		#endregion

		/// <summary>
		/// 
		/// </summary>
		public SFTPクライアント()
		{
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="username"></param>
		/// <param name="password"></param>
		public SFTPクライアント(string ホスト名, int ポート, string ユーザ名, string パスワード)
			: this()
		{
			host = ホスト名;
			port = ポート;
			username = ユーザ名;
			password = パスワード;
			client = new SftpClient(ホスト名, ユーザ名, パスワード);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="username"></param>
		/// <param name="password"></param>
		public SFTPクライアント(string ホスト名, int ポート, string ユーザ名, string パスワード, string プライベートキーファイル)
			: this()
		{
			host = ホスト名;
			port = ポート;
			username = ユーザ名;
			password = パスワード;
			privateKeyFile = プライベートキーファイル;
			PrivateKeyFile pkfile = new PrivateKeyFile(プライベートキーファイル, パスワード);
			client = new SftpClient(ホスト名, ユーザ名, pkfile);
		}

		#region 手順
		/// <summary>サーバ(リモート)にあるファイルをダウンロードします。</summary>
		/// <remarks>【自分】で、【リモートパス】を、【ローカルパス】へ</remarks>
		[自分で]
		public void ダウンロード([を]string リモートパス, [へ] string ローカルパス)
		{
			//ダウンロードしたファイルを書き込むためのFileStreamを作成
			FileStream localStream = new FileStream(ローカルパス, FileMode.Create, FileAccess.Write);
			//ダウンロードしたデータを書き込む
			client.DownloadFile(リモートパス, localStream);
			localStream.Close();
		}

		/// <summary>ローカルにあるファイルをサーバへアップロードします。</summary>
		/// <remarks>【自分】で、【ローカルパス】を、【リモートパス】へ</remarks>
		[自分で]
		public void アップロード([を]string ローカルパス, [へ]string リモートパス)
		{
			string localFileName = Path.GetFileName(ローカルパス);
			string remoteFileName = Path.GetFileName(リモートパス);
			if (remoteFileName.Length == 0) リモートパス += localFileName;

			//アップロードするファイルを開く
			FileStream localStream = new FileStream(ローカルパス, FileMode.Open, FileAccess.Read);
			//アップロードStreamに書き込む
			client.UploadFile(localStream, リモートパス);
			localStream.Close();
		}

		/// <summary>サーバにあるファイルの名前を変更します。</summary>
		/// <remarks>【自分】で、【リモートパス】を、【新しいファイル名】へ</remarks>
		[自分で]
		public void 変更([を]string リモートパス, [へ]string 新しいファイル名)
		{
			try
			{
				client.RenameFile(リモートパス, 新しいファイル名);
			}
			catch (Renci.SshNet.Common.SftpPathNotFoundException)
			{
				throw new ProduireException("パスが見つかりません。");
			}
		}

		/// <summary>サーバにあるファイルを削除します。</summary>
		/// <remarks>【自分】で、【リモートパス】を</remarks>
		[自分で]
		public void 削除([を]string リモートパス)
		{
			try
			{
				client.Delete(リモートパス);
			}
			catch (Renci.SshNet.Common.SftpPathNotFoundException)
			{
				throw new ProduireException("パスが見つかりません。");
			}
		}

		/// <summary>サーバにあるフォルダを削除します。</summary>
		/// <remarks>【自分】で、【リモートパス】という、ディレクトリを</remarks>
		[自分で, 補語("ディレクトリを"), 手順("削除")]
		public void ディレクトリを削除([という]string リモートパス)
		{
			try
			{
				client.DeleteDirectory(リモートパス);
			}
			catch (Renci.SshNet.Common.SftpPathNotFoundException)
			{
				throw new ProduireException("パスが見つかりません。");
			}
		}

		/// <summary>サーバにフォルダを作成します。</summary>
		/// <remarks>【自分】で、【リモートパス】という、ディレクトリを</remarks>
		[自分で, 補語("ディレクトリを"), 手順("作成")]
		public void ディレクトリを作成([という]string リモートパス)
		{
			client.CreateDirectory(リモートパス);
		}
		/// <summary>サーバにフォルダを作成します。</summary>
		/// <remarks>【自分】で、【リモートパス】から、ファイルサイズを</remarks>
		[自分で, 補語("ファイルサイズを"), 手順("取得")]
		public long ファイルサイズを取得([から]string リモートパス)
		{
			try
			{
				return client.GetAttributes(リモートパス).Size;
			}
			catch (Renci.SshNet.Common.SftpPathNotFoundException)
			{
				throw new ProduireException("パスが見つかりません。");
			}
		}

		/// <summary>サーバにあるファイルの更新日時を取得します。</summary>
		/// <remarks>【自分】で、【リモートパス】から、更新日時を</remarks>
		[自分で, 補語("更新日時を"), 手順("取得")]
		public DateTime 更新日時を取得([から]string リモートパス)
		{
			try
			{
				return client.GetLastWriteTime(リモートパス);
			}
			catch (Renci.SshNet.Common.SftpPathNotFoundException)
			{
				throw new ProduireException("パスが見つかりません。");
			}
		}

		/// <summary>サーバの指定したディレクトリにあるファイルの詳細な一覧を取得します。</summary>
		/// <remarks>【自分】で、【リモートパス】から、ファイル詳細一覧を</remarks>
		[自分で, 補語("ファイル詳細一覧を"), 手順("取得")]
		public SFTPファイル[] ファイル詳細一覧を取得([から]string リモートパス)
		{
			IEnumerable<ISftpFile> list;
			try
			{
				list = client.ListDirectory(リモートパス);
			}
			catch (Renci.SshNet.Common.SftpPathNotFoundException)
			{
				throw new ProduireException("パスが見つかりません。");
			}

			List<SFTPファイル> files = new List<SFTPファイル>();
			foreach (SftpFile sftpfile in list)
			{
				if (!sftpfile.IsRegularFile) continue;
				files.Add(new SFTPファイル(sftpfile));
			}

			return files.ToArray();
		}
		/// <summary>サーバの指定したディレクトリにあるファイルの一覧を取得します。</summary>
		/// <remarks>【自分】で、【リモートパス】から、ファイル一覧を</remarks>
		[自分で, 補語("ファイル一覧を"), 手順("取得")]
		public string[] ファイル一覧を取得([から]string リモートパス)
		{
			IEnumerable<ISftpFile> list;
			try
			{
				list = client.ListDirectory(リモートパス);
			}
			catch (Renci.SshNet.Common.SftpPathNotFoundException)
			{
				throw new ProduireException("パスが見つかりません。");
			}

			List<string> files = new List<string>();
			foreach (SftpFile sftpfile in list)
			{
				if (!sftpfile.IsRegularFile) continue;
				files.Add(sftpfile.Name);
			}

			return files.ToArray();
		}
		/// <summary>サーバの指定したディレクトリにあるファイルの一覧を取得します。</summary>
		/// <remarks>【自分】で、【リモートパス】から、ファイル一覧を</remarks>
		[自分で, 補語("ディレクトリ一覧を"), 手順("取得")]
		public string[] ディレクトリ一覧を取得([から]string リモートパス)
		{
			IEnumerable<ISftpFile> list;
			try
			{
				list = client.ListDirectory(リモートパス);
			}
			catch (Renci.SshNet.Common.SftpPathNotFoundException)
			{
				throw new ProduireException("パスが見つかりません。");
			}

			List<string> directories = new List<string>();
			foreach (SftpFile sftpfile in list)
			{
				if (!sftpfile.IsDirectory) continue;
				directories.Add(sftpfile.Name);
			}

			return directories.ToArray();
		}

		/// <summary>サーバの指定したディレクトリにあるファイルの一覧を取得します。</summary>
		/// <remarks>【自分】で、【リモートパス】から、ファイル一覧を</remarks>
		[自分で, 補語("ディレクトリ詳細一覧を"), 手順("取得")]
		public SFTPファイル[] ディレクトリ詳細一覧を取得([から]string リモートパス)
		{
			IEnumerable<ISftpFile> list;
			try
			{
				list = client.ListDirectory(リモートパス);
			}
			catch (Renci.SshNet.Common.SftpPathNotFoundException)
			{
				throw new ProduireException("パスが見つかりません。");
			}

			List<SFTPファイル> directories = new List<SFTPファイル>();
			foreach (SftpFile sftpfile in list)
			{
				if (!sftpfile.IsDirectory) continue;
				directories.Add(new SFTPファイル(sftpfile));
			}

			return directories.ToArray();
		}

		/// <summary>サーバの指定したディレクトリにあるファイルの詳細な一覧を取得します。</summary>
		/// <remarks>【自分】で、【リモートパス】から、ファイル詳細一覧を</remarks>
		[自分で, 補語("詳細一覧を"), 手順("取得")]
		public SFTPファイル[] 詳細一覧を取得([から]string リモートパス)
		{
			IEnumerable<ISftpFile> list;
			try
			{
				list = client.ListDirectory(リモートパス);
			}
			catch (Renci.SshNet.Common.SftpPathNotFoundException)
			{
				throw new ProduireException("パスが見つかりません。");
			}

			List<SFTPファイル> files = new List<SFTPファイル>();
			foreach (SftpFile sftpfile in list)
			{
				files.Add(new SFTPファイル(sftpfile));
			}

			return files.ToArray();
		}

		/// <summary>FTPサーバに接続します。</summary>
		/// <remarks>【自分】を</remarks>
		[自分を]
		public void 接続()
		{
			if (client != null)
			{
				切断();
				client = null;
			}
			if (client == null)
			{
				if (string.IsNullOrEmpty(privateKeyFile))
					client = new SftpClient(host, username, password);
				else
				{
					PrivateKeyFile pkfile = new PrivateKeyFile(privateKeyFile, password);
					client = new SftpClient(host, username, pkfile);
				}
			}
			client.Connect();
		}
		/// <summary>FTPサーバから切断します。</summary>
		/// <remarks>【自分】を</remarks>
		[自分を]
		public void 切断()
		{
			client.Disconnect();
		}
		#endregion

		#region 設定項目
		/// <summary>FTPサーバのホスト名</summary>
		public string サーバ名
		{
			get { return host; }
			set { host = value; }
		}
		/// <summary>SFTPサーバのポート番号</summary>
		public int ポート
		{
			get { return port; }
			set { port = value; }
		}
		/// <summary>SFTPサーバのアカウントのユーザ名</summary>
		public string ユーザ名
		{
			get { return username; }
			set { username = value; }
		}
		/// <summary>SFTPサーバのアカウントのパスワード</summary>
		public string パスワード
		{
			get { return password; }
			set { password = value; }
		}
		/// <summary>SFTPサーバのアカウントのプライベートキー</summary>
		public string プライベートキーファイル
		{
			get { return privateKeyFile; }
			set { privateKeyFile = value; }
		}
		/// <summary>SFTPサーバの作業ディレクトリ</summary>
		public string 現在ディレクトリ
		{
			get { return client.WorkingDirectory; }
		}
		/// <summary>SFTPサーバからの応答コード</summary>
		public bool 接続中
		{
			get { return client.IsConnected; }
		}
		/// <summary>SFTPプロトコルのバージョン</summary>
		public int プロトコルバージョン
		{
			get { return client.ProtocolVersion; }
		}
		/// <summary>接続保持間隔</summary>
		public TimeSpan 接続保持間隔
		{
			get { return client.KeepAliveInterval; }
			set { client.KeepAliveInterval = value; }
		}
		/// <summary>操作タイムアウト時間</summary>
		public TimeSpan 操作タイムアウト時間
		{
			get { return client.OperationTimeout; }
			set { client.OperationTimeout = value; }
		}
		/// <summary>バッファサイズ</summary>
		public uint バッファサイズ
		{
			get { return client.BufferSize; }
			set { client.BufferSize = value; }
		}
		#endregion
	}

	public class SFTPファイル : IProduireClass
	{
		SftpFile sftpfile;
		public SFTPファイル(SftpFile sftpfile)
		{
			this.sftpfile = sftpfile;
		}
		public string ファイル名
		{
			get { return sftpfile.FullName; }
		}
		public long サイズ
		{
			get { return sftpfile.Length; }
		}
		public bool ブロックデバイス
		{
			get { return sftpfile.IsBlockDevice; }
		}
		public bool キャラクタデバイス
		{
			get { return sftpfile.IsCharacterDevice; }
		}
		//[形容詞]
		public bool ディレクトリ
		{
			get { return sftpfile.IsDirectory; }
		}
		public bool ファイル
		{
			get { return sftpfile.IsRegularFile; }
		}
		public bool シンボリックリンク
		{
			get { return sftpfile.IsSymbolicLink; }
		}
		public bool ソケット
		{
			get { return sftpfile.IsSocket; }
		}
		public bool 名前付きパイプ
		{
			get { return sftpfile.IsNamedPipe; }
		}
		public short パーミッション
		{
			set { sftpfile.SetPermissions(value); }
		}
		public DateTime 最終更新日時
		{
			get { return sftpfile.LastWriteTime; }
		}
		public DateTime 最終アクセス日時
		{
			get { return sftpfile.LastAccessTime; }
		}
		public int グループID
		{
			get { return sftpfile.GroupId; }
			set { sftpfile.GroupId = value; }
		}
		public int ユーザID
		{
			get { return sftpfile.UserId; }
			set { sftpfile.UserId = value; }
		}
		public bool グループ読込
		{
			get { return sftpfile.GroupCanRead; }
			set { sftpfile.GroupCanRead = value; }
		}
		public bool グループ書込
		{
			get { return sftpfile.GroupCanWrite; }
			set { sftpfile.GroupCanWrite = value; }
		}
		public bool グループ実行
		{
			get { return sftpfile.GroupCanExecute; }
			set { sftpfile.GroupCanExecute = value; }
		}
		public bool 所有者読込
		{
			get { return sftpfile.OwnerCanRead; }
			set { sftpfile.OwnerCanRead = value; }
		}
		public bool 所有者書込
		{
			get { return sftpfile.OwnerCanWrite; }
			set { sftpfile.OwnerCanWrite = value; }
		}
		public bool 所有者実行
		{
			get { return sftpfile.OwnerCanExecute; }
			set { sftpfile.OwnerCanExecute = value; }
		}
		public bool その他読込
		{
			get { return sftpfile.OthersCanRead; }
			set { sftpfile.OthersCanRead = value; }
		}
		public bool その他書込
		{
			get { return sftpfile.OthersCanWrite; }
			set { sftpfile.OthersCanWrite = value; }
		}
		public bool その他実行
		{
			get { return sftpfile.OthersCanExecute; }
			set { sftpfile.OthersCanExecute = value; }
		}
		[自分を]
		public void 更新する()
		{
			sftpfile.UpdateStatus();
		}
	}
}
