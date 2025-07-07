using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using XGame.Update;

public class UpdateMessageBoxUI : MonoBehaviour
{
    public Text title;
    public Text message;
    public Button btnOk;
    public Text btnOkText;
    public Button btnCancel;
    public Text btnCancelkText;

    //国际化配置
    public UpdateI18NConfig il8nConfig;

    public delegate void OnClickDelegate();
    private OnClickDelegate onOkClick;
    private OnClickDelegate onCancelClick;

    private bool isCloseOnClick;

    public void Show(string msgStr, OnClickDelegate onOkClick = null, OnClickDelegate onCancelClick = null, string titleStr = "版本更新", bool isCloseOnClick = true)
    {
        this.isCloseOnClick = isCloseOnClick;
        this.onOkClick = onOkClick;
        this.onCancelClick = onCancelClick;
        Show(titleStr, msgStr);
    }

    private void Show(string titleStr, string msgStr)
    {



        Debug.LogError("【Update场景弹窗】" + msgStr);
        title.text = il8nConfig.GetLangString(titleStr);
        btnOkText.text = il8nConfig.GetLangString(btnOkText.text);
        btnCancelkText.text = il8nConfig.GetLangString(btnCancelkText.text);

        message.text = msgStr;
        btnCancel.gameObject.BetterSetActive(onCancelClick != null);
        gameObject.BetterSetActive(true);
    }

    public void OnOkClick()
    {
        if (isCloseOnClick)
            Close();
        onOkClick?.Invoke();
    }

    public void OnCancelClick()
    {
        if (isCloseOnClick)
            Close();
        onCancelClick?.Invoke();
    }


    public void Close()
    {
        gameObject.BetterSetActive(false);
    }
}