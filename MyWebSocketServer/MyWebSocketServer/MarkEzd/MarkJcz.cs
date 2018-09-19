using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.Timers;

namespace MarkEzd
{
    public class MarkJcz : MarkEzd_Dll
    {
        private static bool mIsInitLaser = false;
        private static IntPtr mPtr ;
        private static Thread threadInitLaser;
        private static string mMarkEzdFile = null;
        private static LmcErrCode mLastError;
        private static System.Timers.Timer mTimer = null;//= new System.Threading.Timer();

        /// <summary>
        /// 获取当前错误
        /// </summary>
        /// <returns></returns>
        public static LmcErrCode GetLastError()
        {
            return mLastError;
        }


        /// <summary>
        ///JCZ接口初始化
        /// </summary>
        /// <MethodName>InitLaser</MethodName>
        /// <param name="hwnd">hwnd 控件句柄</param>
        /// <returns>true 成功 false 失败</returns>
        public static bool InitLaser(IntPtr hwnd)
        {
            mIsInitLaser = false;
            mPtr = hwnd;
            string strEzCadPath = Application.StartupPath + "\\";
            LmcErrCode Ret = LMC1_INITIAL(strEzCadPath, 0, hwnd);
            if (LmcErrCode.LMC1_ERR_SUCCESS == Ret)
            {  
                mIsInitLaser = true;
                return true;
            }
            else
            {
                mLastError = Ret;
                return false;
            }
        }

        /// <summary>
        ///关闭金橙子板卡
        /// </summary>
        /// <MethodName>Close</MethodName>
        public static void Close()
        {
            if (mIsInitLaser)
            {
                LmcErrCode Ret = LMC1_CLOSE();
                if (LmcErrCode.LMC1_ERR_SUCCESS == Ret)
                {
                    mIsInitLaser = false;
                    return;
                }
                else
                {
                    mLastError = Ret;
                    return;
                }
            }
        }

        /// <summary>
        ///打开指定EZD文件夹
        /// </summary>
        /// <MethodName>LoadEzdFile</MethodName>
        /// <param name="strFile">欲打开EZD文件名</param>
        /// <param name="bDialog">false 手动选择 true 自动加载</param>
        /// <returns>true-成功 fase-失败</returns>
        public static bool LoadEzdFile(ref string strFile, bool bDialog = false)
        {
            if (!mIsInitLaser)
            {
                MessageBox.Show("激光器没有初始化");
                return false;
            }
            if (bDialog)
            {
                OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
                openFileDialog1.Filter = "模板文件(*.ezd)|*.ezd";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    strFile = openFileDialog1.FileName;
                }
                else
                {
                    return false;
                }
            }
            LmcErrCode Ret = LMC1_LOADEZDFILE(strFile);
            if (LmcErrCode.LMC1_ERR_SUCCESS == Ret)
            {       
                mMarkEzdFile = strFile;
                return true;
            }
            else
            {
                mLastError = Ret;
                return false;
            }
        }

        /// <summary>
        /// 标刻
        /// </summary>
        /// <MethodName>Mark</MethodName>
        /// <param name="bFlay">false-静止打标</param>
        /// <returns>true-成功 fase-失败		</returns>
        public static bool Mark(bool bFlay = false)
        {
            if (!mIsInitLaser)
            {
                MessageBox.Show("激光器没有初始化");
                return false;
            }
            int nFly = 0;
            if (bFlay)
            {
                nFly = 1;
            }
            LmcErrCode Ret = LMC1_MARK(nFly);
            if (LmcErrCode.LMC1_ERR_SUCCESS == Ret)
                return true;
            else
            {
                mLastError = Ret;
                return false;
            }
        }

        /// <summary>
        ///停止标刻
        /// </summary>
        /// <MethodName>StopMark</MethodName>
        /// <returns>true-成功 fase-失败s</returns>
        public static bool StopMark()
        {
            if (!mIsInitLaser)
            {
                MessageBox.Show("激光器没有初始化");
                return true;
            }
            LmcErrCode Ret = LMC1_STOPMARK();
            if (LmcErrCode.LMC1_ERR_SUCCESS == Ret)
                return true;
            else
            {
                mLastError = Ret;
                return false;
            }
        }

        /// <summary>
        ///改变对象名字
        /// </summary>
        /// <methodName>ChangeTextByName</methodName>
        /// <param name="strEntName">模板中对象名字</param>
        /// <param name="strText">改变对象的目标文字</param>
        /// <returns>true-成功 fase-失败s</returns>
        public static bool ChangeTextByName(string strEntName, string strText)
        {
            if (!mIsInitLaser)
            {
                MessageBox.Show("激光器没有初始化");
                return false;
            }
            LmcErrCode Ret = LMC1_CHANGETEXTBYNAME(strEntName, strText);

            if (LmcErrCode.LMC1_ERR_SUCCESS == Ret)
                return true;
            else
            {
                mLastError = Ret;
                return false;
            }
        }

        /// <summary>
        ///获取输入IO信号
        /// </summary>
        /// <methodName>ReadPort</methodName>
        /// <param name="nPort">输入端口值</param>
        /// <returns>true-成功 fase-失败</returns>
        public static bool ReadPort(int nPort)
        {
            if (!mIsInitLaser)
            {
                MessageBox.Show("激光器没有初始化");
                return false;
            }
            int nState = 0;
            LmcErrCode Ret = LMC1_READPORT(ref nState);
            if (LmcErrCode.LMC1_ERR_SUCCESS != Ret)
            {
                mLastError = Ret;
                return false;
            }
            if (((nState >> nPort) & 0x01) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///设置输出IO信号 
        /// </summary>
        /// <methodName>SetOutPort</methodName>
        /// <param name="nPort">端口号</param>
        /// <param name="bState">状态值</param>
        /// <returns>true 成功 false 失败</returns>
        public static bool SetOutPort(int nPort, bool bState)
        {
            if (!mIsInitLaser)
            {
                MessageBox.Show("激光器没有初始化");
                return false;
            }
            if (nPort < 0 || nPort > 15)
            {
                return false;
            }
            int nState = 0;
            LmcErrCode Ret = LMC1_GETOUTPORT(ref nState);
            if (LmcErrCode.LMC1_ERR_SUCCESS != Ret)
            {
                mLastError = Ret;
                return false;
            }

            int dbuff = 0;
            if (bState)
            {
                dbuff = 0x0001 << nPort;
                nState |= dbuff;
            }
            else
            {
                dbuff = ~(0x0001 << nPort);
                nState &= dbuff;
            }

            Ret = LMC1_WRITEPORT(nState);
            if (LmcErrCode.LMC1_ERR_SUCCESS == Ret)
                return true;
            else
            {
                mLastError = Ret;
                return false;
            }
            return true;
        }

        /// <summary>		
        /// 平移所有对象到相对位置
        /// </summary>
        /// <methodName>MoveAllEnt</methodName>
        /// <param name="x">X方向距离</param>
        /// <param name="y">Y方向距离</param>
        /// <returns>true 成功 false 失败</returns>
        public static bool MoveAllEnt(double x, double y, double angle = 0)
        {
            if (!mIsInitLaser)
            {
                MessageBox.Show("激光器没有初始化");
                return false;
            }
            int nCount = LMC1_GETENTITYCOUNT();
            if (nCount < 1)
            {
                return false;
            }
            for (int i = 0; i < nCount; i++)
            {
                string strName = "";
                if (!GetEntityName(i, ref strName))
                {
                    return false;
                }
                if (!SetEntityName(i, "shanchu"))
                {
                    return false;
                }
                if (angle == 0)
                {
                    double centerx = 0, centery = 0;
                    GetEntCenter("shanchu", ref centerx, ref centery);
                    RotateEnt("shanchu", centerx, centery, angle);
                }
            
                if (!MoveEnt("shanchu", x, y))
                {
                    return false;
                }
                if (!SetEntityName(i, strName))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>		
        /// 显示所有对象名字到ListView中
        /// </summary>
        /// <methodName>ShowAllEntList</methodName>
        /// <param name="listView">用于输出的ListView</param>
        /// <returns>true 成功 false 失败</returns>
        public static void ShowAllEntList(ListView listView)
        {
            int nCount = MarkJcz.GetEntCount();
            if (nCount <= 0)
            {
                return;
            }
            listView.BeginUpdate();  
            listView.Items.Clear();
            for (int i = 0; i < nCount; i++)
            {
                string strEntName = "";
                MarkJcz.GetEntityName(i, ref strEntName);

                // 添加一行  
                ListViewItem lvItem;
                lvItem = new ListViewItem();
                lvItem.Text = (i+1).ToString();
                listView.Items.Add(lvItem);

                // 添加信息  
                ListViewItem.ListViewSubItem lvSubItem;
                lvSubItem = new ListViewItem.ListViewSubItem();
                lvSubItem.Text = strEntName;
                lvItem.SubItems.Add(lvSubItem);
            }
            listView.EndUpdate();
        }

        /// <summary>
        ///显示图片到Picture控件中
        /// </summary>
        /// <methodName>ShowPreviewBmp</methodName>
        /// <param name="pictureBox">PictureBox 控件</param>
        public static void ShowPreviewBmp(System.Windows.Forms.PictureBox pictureBox)
        {
            if (!mIsInitLaser)
            {
                MessageBox.Show("激光器没有初始化");
                return ;
            }
            int width = pictureBox.Size.Width;
            int height = pictureBox.Size.Height;
            pictureBox.Invoke((EventHandler)(delegate
            {
                //使用GetPrevBitmap2可以使图像显示出来
                IntPtr ptr = LMC1_GETPREVBITMAP2(width, height);
                pictureBox.Image = Bitmap.FromHbitmap(ptr);
                DeleteObject(ptr);
            }));
        }

        /// <summary>
        ///获取所有对象个数
        /// </summary>
        /// <methodName>GetEntCount</methodName>
        /// <returns>模板中对象个数</returns>
        public static int GetEntCount()
        {
            if (!mIsInitLaser)
            {
                MessageBox.Show("激光器没有初始化");
                return -1;
            }
            return LMC1_GETENTITYCOUNT();
        }

        /// <summary>
        ///根据索引获取当前文本对象名字
        /// </summary>
        /// <methodName>GetEntityName</methodName>
        /// <param name="nEntityIndex">nEntityIndex 对象次序</param>
        /// <param name="strEntName">strEntName 对象名字</param>
        /// <returns>true 成功 false 失败</returns>
        public static bool GetEntityName(int nEntityIndex, ref string strEntName)
        {
            char[] chEnt = new char[256];
            LmcErrCode Ret = LMC1_GETENTITYNAME(nEntityIndex, chEnt);

            if (LmcErrCode.LMC1_ERR_SUCCESS == Ret)
            {
                strEntName = new string(chEnt);
                strEntName = strEntName.Replace(new String(new Char[] { '\0' }), "");
                return true;
            }
            else
            {
                mLastError = Ret;
                return false;
            }
        }

        /// <summary>
        ///根据索引设置对象名字
        /// </summary>
        /// <methodName>SetEntityName</methodName>
        /// <param name="nEntityIndex">对象索引</param>
        /// <param name="strEntName">对象名字</param>
        /// <returns>true 成功 false 失败</returns>
        public static bool SetEntityName(int nEntityIndex, string strEntName)
        {
            LmcErrCode Ret = LMC1_SETENTITYNAME(nEntityIndex, strEntName);
            if (LmcErrCode.LMC1_ERR_SUCCESS == Ret)
            {
                SaveEntLibToFile();
                return true;
            }
            else
            {
                mLastError = Ret;
                return false;
            }
        }

        /// <summary>
        ///根据名称获取实体位置和大小
        /// </summary>
        /// <methodName>GetEntSize</methodName>
        /// <param name="strEntName">对象的名字</param>
        /// <param name="dMinx">最小X</param>
        /// <param name="dMiny">最小Y</param>
        /// <param name="dMaxx">最大X</param>
        /// <param name="dMaxy">最大Y</param>
        /// <param name="dZ"></param>
        /// <returns>true 成功 false 失败</returns>
        public static bool GetEntSize(string strEntName, ref double dMinx, ref double dMiny, ref double dMaxx, ref double dMaxy, ref double dZ)
        {
            LmcErrCode Ret = LMC1_GETENTSIZE(strEntName, ref dMinx, ref dMiny, ref dMaxx, ref dMaxy, ref dZ);
            if (LmcErrCode.LMC1_ERR_SUCCESS == Ret)
                return true;
            else
            {
                mLastError = Ret;
                return false;
            }
        }

        /// <summary>
        ///根据对象名字获取中心坐标
        /// </summary>
        /// <methodName>GetEntCenter</methodName>
        /// <param name="strEntName">对象的名字</param>
        /// <param name="dCenx">中心坐标X</param>
        /// <param name="dCeny">中心坐标Y</param>
        /// <returns>true 成功 false 失败</returns>
        public static bool GetEntCenter(string strEntName, ref double dCenx, ref double dCeny)
        {
            dCenx = 0; dCeny = 0;
            double minx = 0, miny = 0, maxx = 0, maxy = 0, rot = 0;
            if (!GetEntSize(strEntName, ref minx, ref miny, ref maxx, ref maxy, ref rot))
            {
                return false;
            }
            dCenx = (maxx + minx) / 2;
            dCeny = (maxy + miny) / 2;
            return true;
        }

        /// <summary>
        ///根据对象名字获取其大小
        /// </summary>
        /// <methodName>GetEntCenter</methodName>
        /// <param name="strEntName">对象的名字</param>
        /// <param name="dCenx">中心坐标X</param>
        /// <param name="dCeny">中心坐标Y</param>
        /// <param name="dWidth">对象宽度</param>
        /// <param name="dHeight">对象高度</param>
        /// <returns>true 成功 false 失败</returns>
        public static bool GetEntCenter(string strEntName, ref double dCenx, ref double dCeny, ref double dWidth, ref double dHeight)
        {
            dCenx = 0; dCeny = 0;
            double minx = 0, miny = 0, maxx = 0, maxy = 0, z = 0;
            if (!GetEntSize(strEntName, ref minx, ref miny, ref maxx, ref maxy, ref z))
            {
                return false;
            }
            dCenx = (maxx + minx) / 2;
            dCeny = (maxy + miny) / 2;
            dWidth = maxx - minx;
            dHeight = maxy - miny;
            return true;
        }


        ///据对象名字设置其大小位置
        /// </summary>
        /// <methodName>SetEntCenter</methodName>
        /// <param name="strEntName">对象的名字</param>
        /// <param name="dCenx">中心坐标X</param>
        /// <param name="dCeny">中心坐标Y</param>
        /// <param name="dWidth">对象宽度</param>
        /// <param name="dHeight">对象高度</param>
        /// <returns>true 成功 false 失败</returns>
        public static bool SetEntCenter(string strEntName, double dCenx, double dCeny, double dWidth, double dHeight)
        {
            double dx = 0, dy = 0;
            double minx = 0, miny = 0, maxx = 0, maxy = 0, z = 0;
            if (!GetEntSize(strEntName, ref minx, ref miny, ref maxx, ref maxy, ref z))
            {
                return false;
            }
            dx = (maxx + minx) / 2;
            dy = (maxy + miny) / 2;

            dCenx -= dx;
            dCeny -= dy;
            if (!MoveEnt(strEntName, dCenx, dCeny))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        ///根据对象名字旋转对象
        /// </summary>
        /// <methodName>RotateEnt</methodName>
        /// <param name="strEntName">对象名</param>
        /// <param name="dCenx">旋转中心X</param>
        /// <param name="dCeny">旋转中心Y</param>
        /// <param name="dAngle">旋转角度(弧度)</param>
        /// <returns>true 成功 false 失败</returns>
        public static bool RotateEnt(string strEntName, double dCenx, double dCeny, double dAngle)
        {
            LmcErrCode Ret = LMC1_ROTATEENT(strEntName, dCenx, dCeny, dAngle);
            if (LmcErrCode.LMC1_ERR_SUCCESS == Ret)
                return true;
            else
            {
                mLastError = Ret;
                return false;
            }
        }

        /// <summary>
        ///根据对象名字移动对象
        /// </summary>
        /// <methodName>MoveEnt</methodName>
        /// <param name="pEntName">对象名</param>
        /// <param name="dMovex">移动X相对距离</param>
        /// <param name="dMovey">移动Y相对距离</param>
        /// <returns>true 成功 false 失败</returns>
        public static bool MoveEnt(string pEntName, double dMovex, double dMovey)
        {
            LmcErrCode Ret = LMC1_MOVEENT(pEntName, dMovex, dMovey);
            if (LmcErrCode.LMC1_ERR_SUCCESS == Ret)
                return true;
            else
            {
                mLastError = Ret;
                return false;
            }
        }

        /// <summary>
        ///删除对象
        /// </summary>
        /// <methodName>DeleteEnt</methodName>
        /// <param name="index">对象索引</param>
        /// <returns>true 成功 false 失败</returns>
        public static bool DeleteEnt(int index)
        {
            int nCount = GetEntCount();
            if (nCount > index)
            {
                SetEntityName(index, "shanchu");
                DeleteEnt("shanchu");
                SaveEntLibToFile();
                return true;
            }
            return false;
        }

        /// <summary>
        ///删除对象
        /// </summary>
        /// <methodName>DeleteEnt</methodName>
        /// <param name="strEntName">对象名</param>
        /// <returns>true 成功 false 失败</returns>
        private static bool DeleteEnt(string strEntName)
        {
            if (!mIsInitLaser)
            {
                MessageBox.Show("激光器没有初始化");
                return false;
            }
            LmcErrCode Ret = lmc1_DeleteEnt(strEntName);
            if (LmcErrCode.LMC1_ERR_SUCCESS == Ret)
            {
                return true;
            }
            else
            {
                mLastError = Ret;
                return false;
            }
        }

        /// <summary>
        ///弹出设置参数对话框
        /// </summary>
        /// <methodName>SetConfigForm</methodName>
        public static void SetConfigForm()
        {
            if (!mIsInitLaser)
            {
                MessageBox.Show("激光器没有初始化");
                return ;
            }
            LmcErrCode Ret = LMC1_SETDEVCFG();
            if (LmcErrCode.LMC1_ERR_SUCCESS == Ret)
            {
                return ;
            }
            else
            {
                mLastError = Ret;
                return ;
            }
        }

        /// <summary>
        ///根据笔号获取所有
        /// </summary>
        /// <methodName>GetPenParam</methodName>
        /// <param name="nPenNo">要设置的笔号(0-255)</param>
        /// <param name="nMarkLoop">加工次数</param>
        /// <param name="dMarkSpeed">标刻次数mm/s</param>
        /// <param name="dPowerRatio">功率百分比(0-100%)</param>
        /// <param name="dCurrent">电流A</param>
        /// <param name="nFreq">频率HZ</param>
        /// <param name="dQPulseWidth">Q脉冲宽度us</param>
        /// <param name="nStartTC">开始延时us</param>
        /// <param name="nLaserOffTC">激光关闭延时us</param>
        /// <param name="nEndTC">结束延时us</param>
        /// <param name="nPolyTC">拐角延时us</param>
        /// <param name="dJumpSpeed">跳转速度mm/s</param>
        /// <param name="nJumpPosTC">跳转位置延时us</param>
        /// <param name="nJumpDistTC">跳转距离延时us</param>
        /// <param name="dEndComp">末点补偿mm</param>
        /// <param name="dAccDist">加速距离mm</param>
        /// <param name="dPointTime">打点延时 ms</param>
        /// <param name="bPulsePointMode">脉冲点模式</param>
        /// <param name="nPulseNum">脉冲点数目</param>
        /// <param name="dFlySpeed">流水线速度</param>
        /// <returns>true 成功 false 失败</returns>
        public static bool GetPenParam(int nPenNo,
            ref int nMarkLoop,
            ref double dMarkSpeed,
            ref double dPowerRatio,
            ref double dCurrent,
            ref int nFreq,
            ref double dQPulseWidth,
            ref int nStartTC,
            ref int nLaserOffTC,
            ref int nEndTC,
            ref int nPolyTC,
            ref double dJumpSpeed,
            ref int nJumpPosTC,
            ref int nJumpDistTC,
            ref double dEndComp,
            ref double dAccDist,
            ref double dPointTime,
            ref bool bPulsePointMode,
            ref int nPulseNum,
            ref double dFlySpeed)
        {
            LmcErrCode Ret = lmc1_GetPenParam(nPenNo, ref nMarkLoop,//加工次数
             ref dMarkSpeed,//标刻次数mm/s
             ref dPowerRatio,//功率百分比(0-100%)	
             ref dCurrent,//电流A
                ref nFreq,//频率HZ
            ref dQPulseWidth,//Q脉冲宽度us	
             ref nStartTC,//开始延时us
             ref nLaserOffTC,//激光关闭延时us 
                ref nEndTC,//结束延时us
             ref nPolyTC,//拐角延时us   //	
             ref dJumpSpeed, //跳转速度mm/s
             ref nJumpPosTC, //跳转位置延时us
            ref  nJumpDistTC,//跳转距离延时us	
             ref  dEndComp,//末点补偿mm
             ref  dAccDist,//加速距离mm	
             ref  dPointTime,//打点延时 ms						 
            ref  bPulsePointMode,//脉冲点模式 
            ref  nPulseNum,//脉冲点数目
             ref  dFlySpeed);//流水线速度

            if (LmcErrCode.LMC1_ERR_SUCCESS == Ret)
                return true;
            else
            {
                mLastError = Ret;
                return false;
            }
        }

        /// <summary>
        ///保存模板
        /// </summary>
        /// <methodName>SaveEntLibToFile</methodName>
        /// <param name="filename">要保存模板名</param>
        /// <returns>true 成功 false 失败</returns>
        public static bool SaveEntLibToFile(string fileName=null)
        {
            if (!mIsInitLaser)
            {
                mLastError = LmcErrCode.LMC1_ERR_NOINITIAL;
                return false;
            }
            if (fileName == null)
            {
                fileName = mMarkEzdFile;
            }
            LmcErrCode Ret = lmc1_SaveEntLibToFile(fileName);
            if (LmcErrCode.LMC1_ERR_SUCCESS == Ret)
            {
                return true;
            }
            else
            {
                mLastError = Ret;
                return false;
            }
        }

        /// <summary>
        ///定时器事件，触发红光
        /// </summary>
        /// <methodName>StartRedLight</methodName>
        /// <param name="source"></param>
        /// <param name="e"></param>
        /// <returns>true 成功 false 失败</returns>
        private static void StartRedLight(Object source, ElapsedEventArgs e)
        {
            if (!mIsInitLaser)
            {
                mLastError = LmcErrCode.LMC1_ERR_NOINITIAL;
                return;
            }
            LmcErrCode Ret = LMC1_REDLIGHTMARK();
            if (LmcErrCode.LMC1_ERR_SUCCESS == Ret)
                return;
            else
            {
                mLastError = Ret;
                return;
            }
        }

        /// <summary>
        ///开始红光
        /// </summary>
        /// <methodName>StartRed</methodName>
        /// <param name="nMs"></param>
        public static void StartRed(int nMs = 100)
        {
            if (mTimer != null)
            {
                StopRed();
            }
            mTimer = new System.Timers.Timer();

            // Hook up the Elapsed event for the timer.
            mTimer.Elapsed += new ElapsedEventHandler(StartRedLight);

            // Set the Interval to 2 seconds (2000 milliseconds).
            mTimer.Interval = nMs;
            mTimer.Enabled = true;
            mTimer.Start();
        }

        /// <summary>
        ///停止红光  
        /// </summary>
        /// <methodName>StopRed</methodName>
        public static void StopRed()
        {
            if (mTimer != null)
            {
                mTimer.Stop();
                mTimer.Elapsed -= new ElapsedEventHandler(StartRedLight);
                mTimer.Dispose();
                mTimer = null;
            }
        }

        /// <summary>
        ///移动扩展轴
        /// </summary>
        /// <methodName>AxisMoveTo</methodName>
        /// <param name="axis">轴号</param>
        /// <param name="GoalPos">目标位置</param>
        /// <returns>true 成功 false 失败</returns>
        public static bool AxisMoveTo(int axis, double GoalPos)
        {
            LmcErrCode Ret = lmc1_AxisMoveTo(axis, GoalPos);
            if (LmcErrCode.LMC1_ERR_SUCCESS == Ret)
            {
                return true;
            }
            else
            {
                mLastError = Ret;
                return false;
            }
        }

        /// <summary>
        ///根据对象索引获取当前数据信息
        /// </summary>
        /// <methodName>SetParamByName</methodName>
        /// <param name="nIndex">索引</param>
        /// <param name="dSetPower">功率</param>
        /// <param name="nSetRate">频率</param>
        /// <param name="dSetCurrent">电流</param>
        /// <returns>true 成功 false 失败</returns>
        public static bool GetParamByName(int nIndex, ref double dPowerRatio, ref int nFreq, ref double dCurrent)
        {
            string strEntName = null;
            if (!GetEntityName(nIndex, ref strEntName))
            {
                return false;
            }
            if (!SetEntityName(nIndex, "shanchu"))
            {
                return false;
            }
            int nPen = lmc1_GetPenNumberFromEnt("shanchu");
            if (!SetEntityName(nIndex, strEntName))
            {
                return false;
            }
            if (nPen < 0)
            {
                return false;
            }
            int nMarkLoop = 0;//加工次数
            double dMarkSpeed = 0; ;//标刻次数mm/s
            //double dPowerRatio = 0;//功率百分比(0-100%)	
            //double dCurrent = 0;//电流A
            //int nFreq = 0;//频率HZ
            double dQPulseWidth = 0;//Q脉冲宽度us	
            int nStartTC = 0;//开始延时us
            int nLaserOffTC = 0;//激光关闭延时us 
            int nEndTC = 0;//结束延时us
            int nPolyTC = 0;//拐角延时us   //	
            double dJumpSpeed = 0; //跳转速度mm/s
            int nJumpPosTC = 0; //跳转位置延时us
            int nJumpDistTC = 0;//跳转距离延时us	
            double dEndComp = 0;//末点补偿mm
            double dAccDist = 0;//加速距离mm	
            double dPointTime = 0;//打点延时 ms						 
            bool bPulsePointMode = false;//脉冲点模式 
            int nPulseNum = 0;//脉冲点数目
            double dFlySpeed = 0;//流水线速度
            lmc1_GetPenParam(nPen, ref nMarkLoop, ref dMarkSpeed, ref dPowerRatio, ref dCurrent, ref nFreq, ref dQPulseWidth,
                ref nStartTC, ref nLaserOffTC, ref nEndTC, ref nPolyTC, ref dJumpSpeed, ref nJumpPosTC, ref nJumpDistTC, ref dEndComp
                , ref dAccDist, ref dPointTime, ref bPulsePointMode, ref nPulseNum, ref dFlySpeed);
            return true;
        }

        /// <summary>
        ///根据对象索引设置功率，频率，电流 
        /// </summary>
        /// <methodName>SetParamByName</methodName>
        /// <param name="nIndex">索引</param>
        /// <param name="dSetPower">功率</param>
        /// <param name="nSetRate">频率</param>
        /// <param name="dSetCurrent">电流</param>
        /// <returns>true 成功 false 失败</returns>
        public static bool SetParamByName(int nIndex, double dSetPower, int nSetRate, double dSetCurrent)
        {
            if (dSetPower < 0 || dSetPower > 100)
            {
                MessageBox.Show("功率超过实际范围");
                return false;
            }
            string strEntName = null;
            if (!GetEntityName(nIndex, ref strEntName))
            {
                return false;
            }
            if (!SetEntityName(nIndex, "shanchu"))
            {
                return false;
            }

            int nPen = lmc1_GetPenNumberFromEnt("shanchu");
            if (!SetEntityName(nIndex, strEntName))
            {
                return false;
            }
            if (nPen < 0)
            {
                return false;
            }
            int nMarkLoop = 0;//加工次数
            double dMarkSpeed = 0; ;//标刻次数mm/s
            double dPowerRatio = 0;//功率百分比(0-100%)	
            double dCurrent = 0;//电流A
            int nFreq = 0;//频率HZ
            double dQPulseWidth = 0;//Q脉冲宽度us	
            int nStartTC = 0;//开始延时us
            int nLaserOffTC = 0;//激光关闭延时us 
            int nEndTC = 0;//结束延时us
            int nPolyTC = 0;//拐角延时us   //	
            double dJumpSpeed = 0; //跳转速度mm/s
            int nJumpPosTC = 0; //跳转位置延时us
            int nJumpDistTC = 0;//跳转距离延时us	
            double dEndComp = 0;//末点补偿mm
            double dAccDist = 0;//加速距离mm	
            double dPointTime = 0;//打点延时 ms						 
            bool bPulsePointMode = false;//脉冲点模式 
            int nPulseNum = 0;//脉冲点数目
            double dFlySpeed = 0;//流水线速度
            lmc1_GetPenParam(nPen, ref nMarkLoop, ref dMarkSpeed, ref dPowerRatio, ref dCurrent, ref nFreq, ref dQPulseWidth,
                ref nStartTC, ref nLaserOffTC, ref nEndTC, ref nPolyTC, ref dJumpSpeed, ref nJumpPosTC, ref nJumpDistTC, ref dEndComp
                , ref dAccDist, ref dPointTime, ref bPulsePointMode, ref nPulseNum, ref dFlySpeed);

            lmc1_SetPenParam(nPen, nMarkLoop, dMarkSpeed, dSetPower, dSetCurrent, nSetRate, dQPulseWidth,
                 nStartTC, nLaserOffTC, nEndTC, nPolyTC, dJumpSpeed, nJumpPosTC, nJumpDistTC, dEndComp
                , dAccDist, dPointTime, bPulsePointMode, nPulseNum, dFlySpeed);

            return true;
        }

        /// <summary>
        ///根据对象索引获取中心位置大小功率，频率，电流
        /// </summary>
        /// <methodName>GetParam</methodName>
        /// <param name="x">中心 X</param>
        /// <param name="y">中心 Y</param>
        /// <param name="w">宽度</param>
        /// <param name="h">高度</param>
        /// <param name="a">功率</param>
        /// <param name="b">频率</param>
        /// <param name="c">电流</param>
        /// <returns>true 成功 false 失败</returns>
        public static bool GetParam(int nIndex, ref double x, ref double y, ref double w, ref double h, ref double a, ref int b, ref double c)
        {
            string strName = null;
            if (!GetEntityName(nIndex, ref strName))
            {
                return false;
            }
            if (!SetEntityName(nIndex, "shanchu"))
            {
                return false;
            }
            GetEntCenter("shanchu", ref x, ref y, ref w, ref h);
            if (!SetEntityName(nIndex, strName))
            {
                return false;
            }
            if (!GetParamByName(nIndex, ref a, ref b, ref c))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        ///根据对象索引获取中心位置大小功率，频率，电流
        /// </summary>
        /// <methodName>SetParam</methodName>
        /// <param name="x">中心 X</param>
        /// <param name="y">中心 Y</param>
        /// <param name="w">宽度</param>
        /// <param name="h">高度</param>
        /// <param name="a">功率</param>
        /// <param name="b">频率</param>
        /// <param name="c">电流</param>
        /// <returns>true 成功 false 失败</returns>
        public static bool SetParam(int nIndex, double x, double y, double w, double h, double a, int b, double c)
        {
            string strName = null;
            if (!GetEntityName(nIndex, ref strName))
            {
                return false;
            }
            if (!SetEntityName(nIndex, "shanchu"))
            {
                return false;
            }
            SetEntCenter("shanchu", x, y, w, h);
            if (!SetEntityName(nIndex, strName))
            {
                return false;
            }
            if (!SetParamByName(nIndex, a, b, c))
            {
                return false;
            }
            return true;
        }
    }
}
