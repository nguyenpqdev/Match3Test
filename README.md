1. Optimize
   -  Object Pooling cho Items: Tạo một ItemPool class để quản lý việc tái sử dụng các Item thay vì tạo mới và hủy liên tục.
   -  Cache cho kết quả tìm kiếm match:Trong file Board.cs Thêm Dictionary matchCache để lưu trữ kết quả của các lần tìm kiếm match .
   -  Tối ưu hóa Animation với DOTween Sequence:
     ``` csharp
         Sequence swapSequence = DOTween.Sequence();
        swapSequence.Join(item.View.DOMove(cell2.transform.position, 0.3f));
        swapSequence.Join(item2.View.DOMove(cell1.transform.position, 0.3f));
        swapSequence.OnComplete(() => { if (callback != null) callback(); });
     ```

2. Reskin
   - Tạo script ItemSkinData.cs , Tạo ScriptableObject ItemSkinData.asset trong thư mục Resources/Data/ 
   - Setting type với sprite tương ứng
   - Fix code trong file NormalItem.cs:
    ``` csharp
     public static ItemSkinData currentSkin;
     public override void SetView()
        {
            base.SetView();
            if (View != null && currentSkin != null)
            {
                SpriteRenderer spriteRenderer = View.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = currentSkin.GetSpriteForType(ItemType);
                }
            }
        }
     ```
   - Load Data ScriptableObject tại file GameManage.cs trong Awake();
     ``` csharp
     NormalItem.currentSkin = Resources.Load<ItemSkinData>("Data/ItemSkinData");
     ```
3. Button Restart
   - Sửa file UIPanelGameOver.cs:
   - lưu lại mode level cuối cùng được chọn: GameManager.eLevelMode m_lastLevelMode;
     ``` csharp
        [SerializeField] private Button btnRestart;
        private GameManager.eLevelMode m_lastLevelMode;
     .....
      private void Awake()
    {
        btnClose.onClick.AddListener(OnClickClose);
        btnRestart.onClick.AddListener(OnClickRestart);
    }
   
    private void OnDestroy()
    {
        if (btnClose) btnClose.onClick.RemoveAllListeners();
        if (btnRestart) btnRestart.onClick.RemoveAllListeners();
    }
    private void OnClickRestart()
    {
        m_mngr.RestartLevel(m_lastLevelMode);
    }
     ```

- thêm func RestartLevel trong file UiMainManager.cs:
  ``` csharp
   internal void RestartLevel(GameManager.eLevelMode mode)
    {
        m_gameManager.RestartLevel(mode);
    }
  ```
- GameManager:
  Thêm biến m_currentLevelMode để lưu mode level hiện tại
  Thêm hàm RestartLevel để:
  ``` csharp
  
    public void RestartLevel(eLevelMode mode)
    {
        ClearLevel();
        SetState(eStateGame.RESTART);
        LoadLevel(mode);
    }
  ```
  Xóa level hiện tại
  Chuyển sang trạng thái RESTART
  Load lại level với mode tương ứng
  ``` csharp
   public void LoadLevel(eLevelMode mode)
    {
        if(m_boardController != null) Destroy(m_boardController.gameObject);
        m_currentLevelMode = mode;
       ......
    }
  ```
4. FillGapsWithNewItems :
   ``` csharp
   internal void FillGapsWithNewItems()
    {
        ClearMatchCache();
        Sequence fillSequence = DOTween.Sequence();
    
        Dictionary<NormalItem.eNormalType, int> typeCounts = new Dictionary<NormalItem.eNormalType, int>();
        foreach (var cell in m_cells)
        {
            if (cell.Item is NormalItem normal)
            {
                if (!typeCounts.ContainsKey(normal.ItemType))
                    typeCounts[normal.ItemType] = 0;
                typeCounts[normal.ItemType]++;
            }
        }

        NormalItem.eNormalType leastCommonType = NormalItem.eNormalType.TYPE_ONE;
        int minCount = int.MaxValue;
        foreach (var kvp in typeCounts)
        {
            if (kvp.Value < minCount)
            {
                minCount = kvp.Value;
                leastCommonType = kvp.Key;
            }
        }

        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                if (!cell.IsEmpty) continue;

                List<NormalItem.eNormalType> neighborTypes = new List<NormalItem.eNormalType>();
                
                if (cell.NeighbourUp != null && cell.NeighbourUp.Item is NormalItem upItem)
                    neighborTypes.Add(upItem.ItemType);
                if (cell.NeighbourRight != null && cell.NeighbourRight.Item is NormalItem rightItem)
                    neighborTypes.Add(rightItem.ItemType);
                if (cell.NeighbourBottom != null && cell.NeighbourBottom.Item is NormalItem bottomItem)
                    neighborTypes.Add(bottomItem.ItemType);
                if (cell.NeighbourLeft != null && cell.NeighbourLeft.Item is NormalItem leftItem)
                    neighborTypes.Add(leftItem.ItemType);

                NormalItem item = new NormalItem();
                if (!neighborTypes.Contains(leastCommonType))
                {
                    item.SetType(leastCommonType);
                }
                else
                {
                    List<NormalItem.eNormalType> availableTypes = new List<NormalItem.eNormalType>();
                    foreach (var kvp in typeCounts)
                    {
                        if (!neighborTypes.Contains(kvp.Key))
                        {
                            availableTypes.Add(kvp.Key);
                        }
                    }

                    if (availableTypes.Count > 0)
                    {
                        int randomIndex = UnityEngine.Random.Range(0, availableTypes.Count);
                        item.SetType(availableTypes[randomIndex]);
                    }
                    else
                    {
                        item.SetType(Utils.GetRandomNormalTypeExcept(neighborTypes.ToArray()));
                    }
                }

                item.SetView();
                item.SetViewRoot(m_root);

                cell.Assign(item);
                cell.ApplyItemPosition(true);

                if (!typeCounts.ContainsKey(item.ItemType))
                    typeCounts[item.ItemType] = 0;
                typeCounts[item.ItemType]++;
                
                fillSequence.Join(item.View.DOScale(Vector3.one, 0.1f).From(Vector3.one * 0.1f));
            }
        }
    }
   ```
   - Đếm số lượng của từng loại item trên bàn chơi
   - Tìm loại item có số lượng ít nhất
   - Lấy danh sách các loại item của 4 ô xung quanh
   - Ưu tiên chọn loại item ít xuất hiện nhất
   - Chọn ngẫu nhiên từ các loại item có sẵn
   - Cập nhật số lượng item sau khi tạo mới

5. Nhận xét:
   -   Phân chia thư mục rõ ràng:
   Có các thư mục riêng biệt cho Board, Controllers, UI, Utilities
   Dễ dàng tìm kiếm và quản lý code theo chức năng
   -   Sử dụng ScriptableObject:
   GameSettings và ItemSkinData sử dụng ScriptableObject
   Giúp quản lý dữ liệu game dễ dàng trong Editor
   Có thể tạo nhiều cấu hình khác nhau
   -  Tách biệt logic:
   Board logic được tách riêng khỏi UI
   Có các controller riêng để quản lý game flow
   Item system được thiết kế theo hướng OOP

   


     
     
       





