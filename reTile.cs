using System;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Drawing.Imaging;
using Microsoft.VisualBasic.FileIO;

class CReTile
{
	string sTableName;
	string sOutFolder;

	int    nOutPixlWidth  = 0;
	int    nOutScale      = 0;

	double dOutRealWidth = 0;

	public double dDotParMeter()
	{
		return 96.0 / 0.0254;
	}

	/*
	 * 出力するタイルの空間を求める
	 */
	public void getOutTileSpace(Table tbl, double realWidth, out int loopX, out int loopY, out double minX, out double minY, out double maxX, out double maxY)
	{
		// X開始位置
		double dX1  = tbl.nMinX / realWidth;
		int    nX1  = (int)Math.Floor(dX1);
		minX = nX1 * realWidth;

		// Y開始位置
		double dY1  = tbl.nMinY / realWidth;
		int    nY1  = (int)Math.Floor(dY1);
		minY = nY1 * realWidth;

		// X終了位置
		double dX2  = tbl.nMaxX / realWidth;
		int    nX2  = (int)Math.Ceiling(dX2);
		maxX = nX2 * realWidth;

		// Y終了位置
		double dY2  = tbl.nMaxY / realWidth;
		int    nY2  = (int)Math.Ceiling(dY2);
		maxY = nY2 * realWidth;

		loopX = nX2 - nX1;
		loopY = nY2 - nY1;
	}

	static int Main(string[] args)
	{
		//Default
		string sFormatExt = "jpg";
		System.Drawing.Imaging.ImageFormat IMG_FMT = System.Drawing.Imaging.ImageFormat.Jpeg;

		int nameType = 1;	// 1:SIS  2:Sonic

		if (args.Length < 4)
		{
			Console.Error.WriteLine("usage : reTile <処理Table> <出力画像幅(px)> <出力縮尺> <出力先フォルダ> [-gif|-png|-jpg] [-sis|-sonic]");
			Console.Error.WriteLine(" ※処理Tableの書式（カンマ区切）");
			Console.Error.WriteLine("   D:\\image\\picA.jpg, 12000, 10000, 12200, 10200");
			Console.Error.WriteLine("   D:\\image\\picB.jpg, 12200, 10000, 12200, 10200");
			Console.Error.WriteLine(" ※出力先フォルダは予め用意して下さい。");
			Console.Error.WriteLine("\n copyright (c) 2011 geonext.co.jp");
			return 1;
		}

		Console.Error.WriteLine("c={0} {1} {2} {3} {4}",args.Length, args[0], args[1], args[2], args[3]);

		if (args.Length > 4)
		{
			for (int n=3; n < args.Length - 1; n++)
			{
				int param = n + 1;
				if (args[param].ToLower() == "-gif")
				{
					sFormatExt = "gif";
					IMG_FMT = System.Drawing.Imaging.ImageFormat.Gif;
				}
				else if (args[param].ToLower() == "-png")
				{
					sFormatExt = "png";
					IMG_FMT = System.Drawing.Imaging.ImageFormat.Png;
				}
				else if (args[param].ToLower() == "-jpg")
				{
				}
				else if (args[param].ToLower() == "-sis")
				{
					nameType = 1;
				}
				else if (args[param].ToLower() == "-sonic")
				{
					nameType = 2;
				}
				else
				{
					Console.Error.WriteLine("Parameter error {0}", args[param]);
					return 1;
				}
			}
		}


		CReTile rt = new CReTile();

		rt.sTableName    = args[0];
		rt.nOutPixlWidth = int.Parse(args[1]);
		rt.nOutScale     = int.Parse(args[2]);
		rt.sOutFolder    = args[3];

		int len = rt.sOutFolder.Length;
		// 末尾が\なら消す
		if (rt.sOutFolder[len-1] == '\\')
		{
			rt.sOutFolder = rt.sOutFolder.Substring(0,len-1);
		}

		if (! System.IO.Directory.Exists(rt.sOutFolder))
		{
			Console.WriteLine("出力先Folderがありません {0}", rt.sOutFolder);
			return 1;
		}

		// 完成するタイルの実測幅
		rt.dOutRealWidth = rt.nOutScale * rt.nOutPixlWidth / rt.dDotParMeter();

		// 完成するタイルの解像度(Dot/Meter)
		double dstDPM_W = rt.nOutPixlWidth / rt.dOutRealWidth;
		double dstDPM_H = rt.nOutPixlWidth / rt.dOutRealWidth;

		Console.WriteLine("完成するタイルの実測幅(Meter) {0}", rt.dOutRealWidth);
		Console.WriteLine("完成するタイルの解像度(Dot/Meter) {0}", dstDPM_W);

		Console.WriteLine("{0} {1} {2} {3} {4}", args[0], rt.nOutPixlWidth, rt.nOutScale, rt.dOutRealWidth, rt.sOutFolder);

		Table tbl = new Table(rt.sTableName);
		ArrayList list = tbl.getList();

//		foreach(tableData item in list){
//			Console.WriteLine(">>{0} {1} {2}", item.name, item.rx1, item.ry1);
//		}

		double minX, minY, maxX, maxY;
		int loopX, loopY;
		rt.getOutTileSpace(tbl, rt.dOutRealWidth, out loopX, out loopY, out minX, out minY, out maxX, out maxY);
		Console.WriteLine("loopX  {0} ",  loopX);
		Console.WriteLine("loopY  {0} ",  loopY);
		Console.WriteLine("orginX {0} ",  minX );
		Console.WriteLine("orignY {0} ",  maxY );

// 66.1458333333333
/*
		for (int nx = 33; nx < 34; nx++)
		{
			for (int ny = 47; ny < 48; ny++)
			{
*/
		for (int nx = 0; nx < loopX; nx++)
		{
			for (int ny = 0; ny < loopY; ny++)
			{

				double dx1 = nx * rt.dOutRealWidth + minX;
				double dy1 = ny * rt.dOutRealWidth + minY;
				double dx2 = dx1 + rt.dOutRealWidth;
				double dy2 = dy1 + rt.dOutRealWidth;
				// 必要なメッシュを取得
				ArrayList L = rt.getTileList(list, dx1, dy1, dx2, dy2);
				// 名前決定
				int nNameX = (int)Math.Round(dx1 / rt.dOutRealWidth);
				int nNameY = (int)Math.Round(dy1 / rt.dOutRealWidth);

//				if (nNameX == -270 && nNameY == -2448)
//				{
//					Console.WriteLine("x={0} y={1} **", nx, ny);return 1;
//				}

				string sDstName = nNameX.ToString("00000;-0000") + "0" +nNameY.ToString("00000;-0000") + "0." + sFormatExt;
				if (nameType == 2)
				{
					sDstName  = nx.ToString() + "_" + ny.ToString() + "." + sFormatExt;
				}

				sDstName = rt.sOutFolder + "\\" + sDstName;

				Console.WriteLine("x={0} y={1} {2} {3} tile count={4}", nx, ny, args[0], sDstName, L.Count);

				/*
				if(L.Count > 6000){
					Console.WriteLine("ERROR OVERFLOW : {0}", L.Count);
					return 1;
				}
				*/

				//エリアに１つでもかかったら出力
				if (L.Count > 0)
				{
					// 出力空間準備
					Bitmap dest = new Bitmap(rt.nOutPixlWidth, rt.nOutPixlWidth);
					Graphics g = Graphics.FromImage(dest);
					// 白で塗る
					Rectangle rc = new Rectangle(0, 0, rt.nOutPixlWidth, rt.nOutPixlWidth);
					g.FillRectangle(Brushes.White, 0, 0, rt.nOutPixlWidth, rt.nOutPixlWidth);

					foreach (tableData d in L)
					{
						Bitmap src = (Bitmap)Bitmap.FromFile(d.name);
						double srcDPM_W = (double)src.Width  / (d.rx2-d.rx1);
						double srcDPM_H = (double)src.Height / (d.ry2-d.ry1);

						//Console.WriteLine("src.Width={0} d.rx1={1} d.rx2={2} 差分={3} 解像度={4}", src.Width, d.rx1, d.rx2, (d.rx2-d.rx1),srcDPM_W);

						// 出力解像度／入力解像度
						double dCnvFacW  = dstDPM_W / srcDPM_W;
						double dCnvFacH  = dstDPM_H / srcDPM_H;

						//Console.WriteLine("{0} rx1={1} ry1={2} rx2={3} ry2={4}", d.name, d.rx1, d.ry1, d.rx2, d.ry2);

						double dCnvFac = dCnvFacW;

						// 解像度算出
						int nDstX1 = (int)Math.Round(dx1 * srcDPM_W);
						int nDstY1 = (int)Math.Round(dy1 * srcDPM_W);

						int pxX  = (int)Math.Round(d.rx1 * srcDPM_W);
						int pxY  = (int)Math.Round(d.ry1 * srcDPM_W);
						int pxX2 = (int)Math.Round(d.rx2 * srcDPM_W);
						int pxY2 = (int)Math.Round(d.ry2 * srcDPM_W);

						// 画像座標
						//Console.WriteLine("{0} ix1={1} iy1={2} ix2={3} iy2={4}", d.name, pxX, pxY, pxX2, pxY2);

						int w2     = (int)Math.Round(src.Width  * dCnvFac) + 1;
						int h2     = (int)Math.Round(src.Height * dCnvFac) + 1;
						int nPosX  = (int)Math.Round(pxX * dCnvFac - nDstX1 * dCnvFac);
						int nPosY  = (int)Math.Round(rt.nOutPixlWidth - (pxY * dCnvFac + src.Height * dCnvFac - nDstY1 * dCnvFac));

						g.DrawImage(src, nPosX, nPosY, w2, h2);
						src.Dispose();
					}
					dest.Save(sDstName, IMG_FMT);
					dest.Dispose();
				}
				else
				{
					Bitmap dest = new Bitmap(rt.nOutPixlWidth, rt.nOutPixlWidth);
					Graphics g = Graphics.FromImage(dest);
					// 白で塗る
					Rectangle rc = new Rectangle(0, 0, rt.nOutPixlWidth, rt.nOutPixlWidth);
					g.FillRectangle(Brushes.White, 0, 0, rt.nOutPixlWidth, rt.nOutPixlWidth);
					dest.Save(sDstName, IMG_FMT);
					dest.Dispose();
				}
			}
		}
		return 0;
	}

	public ArrayList getTileList(ArrayList list, double x1, double y1, double x2, double y2)
	{
		//ArrayList [] retA = new ArrayList[5000];
		ArrayList listA = new ArrayList();
		foreach (tableData d in list) {
			// 検査点が図郭内
			if      (d.rx1 <= x1 && d.rx2 >= x1 && d.ry1 <= y1 && d.ry2 >= y1)	//左下
			{
				listA.Add(d);
			}
			else if (d.rx1 <= x1 && d.rx2 >= x1 && d.ry1 <= y2 && d.ry2 >= y2)	//左上
			{
				listA.Add(d);
			}
			else if (d.rx1 <= x2 && d.rx2 >= x2 && d.ry1 <= y1 && d.ry2 >= y1)	//右下
			{
				listA.Add(d);
			}
			else if (d.rx1 <= x2 && d.rx2 >= x2 && d.ry1 <= y2 && d.ry2 >= y2)	//右上
			{
				listA.Add(d);
			}
			else if (d.rx2 >= x1 && d.rx1 <= x2 && d.ry2 >= y1 && d.ry1 <= y2)
			{
				listA.Add(d);
			}
		}
		return listA;
	}
}

public class Table
{
	public int nMinX  =  99999999;
	public int nMinY  =  99999999;
	public int nMaxX  = -99999999;
	public int nMaxY  = -99999999;
	public double dWidth  = 0.0;
	public double dHeight = 0.0;

	ArrayList list = new ArrayList();
	public Table(string tableFile)
	{
		TextFieldParser parser = new TextFieldParser(tableFile, System.Text.Encoding.GetEncoding("Shift_JIS"));
		parser.TextFieldType = FieldType.Delimited;
		parser.SetDelimiters("=");	// イコールで分割（今は意味なし）
		while (! parser.EndOfData) {
			string [] row = parser.ReadFields();
			if (row.Length == 1)
			{
				// ファイルレコード
				string[] field = row[0].Split(',');// カンマで分割
				int x1 = int.Parse(field[1]);
				int y1 = int.Parse(field[2]);
				int x2 = int.Parse(field[3]);
				int y2 = int.Parse(field[4]);
				nMinX = x1 < nMinX ? x1 : nMinX;
				nMinY = y1 < nMinY ? y1 : nMinY;
				nMaxX = x2 > nMaxX ? x2 : nMaxX;
				nMaxY = y2 > nMaxY ? y2 : nMaxY;
				list.Add(new tableData(field[0], x1, y1, x2, y2));
			}
		}
		parser.Dispose();
	}

	public ArrayList getList()
	{
		return list;
	}
}

public class tableData
{
	private string _name;
	private int _rx1, _ry1, _rx2, _ry2;

	public tableData()
	{
	}

	public tableData(string name, int rx1, int ry1, int rx2, int ry2)
	{
		_name = name;
		_rx1 = rx1;
		_ry1 = ry1;
		_rx2 = rx2;
		_ry2 = ry2;
	}

	public string name
	{
		set{ this._name = value; }
		get{ return this._name;  }
	}

	// 実測座標
	public int rx1
	{
		set{ this._rx1 = value; }
		get{ return this._rx1;  }
	}
	public int ry1
	{
		set{ this._ry1 = value; }
		get{ return this._ry1;  }
	}
	public int rx2
	{
		set{ this._rx2 = value; }
		get{ return this._rx2;  }
	}
	public int ry2
	{
		set{ this._ry2 = value; }
		get{ return this._ry2;  }
	}

}

/*
		parser.TextFieldType = FieldType.Delimited;
		parser.SetDelimiters("=");	// イコールで分割
		while (! parser.EndOfData) {
			string [] row = parser.ReadFields();
			if (row.Length == 2)
			{
				// 実測幅レコード
				if (0 == string.Compare(row[0], "width"))
				{
					dWidth = double.Parse(row[1]);
				}
				// 実測高さレコード
				if (0 == string.Compare(row[0], "height"))
				{
					dHeight = double.Parse(row[1]);
				}
			}
		}
		parser.Close();
		parser.Dispose();
		parser = new TextFieldParser(tableFile, System.Text.Encoding.GetEncoding("Shift_JIS"));
*/
