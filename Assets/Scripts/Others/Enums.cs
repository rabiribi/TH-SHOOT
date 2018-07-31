namespace GameMain
{
	public enum Language
	{
		简体中文=0,
		English=1,
		日本語=2
	}
	/// <summary>
	/// 游戏事件
	/// </summary>
	public enum GameEventType
	{
		/// <summary>
		/// 加载存档
		/// </summary>
		LoadGame = 0,
		/// <summary>
		///存储存档
		/// </summary>
		SaveGame = 1,
		/// <summary>
		/// 从临时数据读取全局存档
		/// </summary>
		ReadGlobalGameData = 2,
		/// <summary>
		/// 向临时数据写入全局存档
		/// </summary>
		RememberGlobalGameData = 3,
		/// <summary>
		/// 从临时数据读取全局存档
		/// </summary>
		RememberRecordData = 4,
		/// <summary>
		/// 向临时数据写入全局存档
		/// </summary>
		ReadRecordData = 5,
		/// <summary>
		/// 确认数据
		/// </summary>
		OptionConfirm = 6
	}
}
