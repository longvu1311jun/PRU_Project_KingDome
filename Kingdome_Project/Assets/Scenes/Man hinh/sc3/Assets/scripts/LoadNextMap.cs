using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadNextMap : MonoBehaviour
{
    public float delaySecond = 2;
    public string nameScene = "Scene3";
    public int requiredGemCount = 1;
    public TextMeshProUGUI gemCountText;

    private void OnTriggerEnter2D(Collider2D collision) // Xử lý sự kiện khi một đối tượng va chạm với collider của GameObject này
    {
        if (collision.gameObject.tag == "Player")
        {
            int collectedGemCount = CountCollectedGems(collision.gameObject); // // Đếm số lượng đá quý mà người chơi đã thu thập


            if (collectedGemCount >= requiredGemCount)
            {
                collision.gameObject.SetActive(false); // ẩn ng chơi 
                ModeSelect();
            }
            else
            {
                // Hiển thị thông báo về số lượng ngọc không đủ
                ShowMessage("Not enough gems! You need at least " + requiredGemCount + " gems.");
            }
            // Add an else statement if you want to do something if the player doesn't have enough gems.
        }
    }

    private int CountCollectedGems(GameObject player) // đếm số lượng đá quý 
    {
        
        XuliVaCham gemCollector = player.GetComponent<XuliVaCham>();  // Lấy component XuliVaCham từ người chơi

        if (gemCollector != null)
        {
            return gemCollector.CollectedGemsCount();    // Nếu component tồn tại, gọi phương thức để lấy số lượng đá quý đã thu thập

        }

        return 0;
    }

    public void ModeSelect() //load màn sau một khoảng thời gian

    {
        StartCoroutine(LoadAfterDelay());
    }

    IEnumerator LoadAfterDelay() //đợi một khoảng thời gian trước khi chuyển màn
    {
        yield return new WaitForSeconds(delaySecond);
        SceneManager.LoadScene(nameScene);
    }

    private void ShowMessage(string message)
    {
        gemCountText.text = message;

        // Gọi StartCoroutine để tạm dừng hiển thị và sau đó xóa thông báo
        StartCoroutine(HideMessageAfterDelay());
    }

    // Hàm IEnumerator để ẩn thông báo sau một khoảng thời gian
    IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(2f); // Chờ 2 giây

        // Xóa thông báo sau khoảng thời gian chờ
        gemCountText.text = "";
    }
}

