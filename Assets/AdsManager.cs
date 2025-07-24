using UnityEngine;
using GoogleMobileAds.Api; // Namespace del plugin de AdMob

public class AdsManager : MonoBehaviour
{
    private BannerView bannerView;

    void Start()
    {
        // Inicializa el SDK de Google Mobile Ads.
        // Esto solo se debe hacer una vez, por eso está en Start().
        MobileAds.Initialize(initStatus => { });

        // Llamamos a la función para solicitar el banner.
        this.RequestBanner();
    }

    private void RequestBanner()
    {
        // ⚠️ MUY IMPORTANTE: USA IDs DE PRUEBA DURANTE EL DESARROLLO ⚠️
        // Usar tus IDs reales mientras pruebas puede suspender tu cuenta de AdMob.
        // Google proporciona IDs de prueba para este propósito.
        
        #if UNITY_ANDROID
            // --- USA ESTE ID PARA PROBAR ---
            string adUnitId = "ca-app-pub-3940256099942544/6300978111"; 
            
            // --- USA TU ID REAL SOLO CUANDO VAYAS A PUBLICAR ---
            // string adUnitId = "ca-app-pub-xxxxxxxxxxxxxxxx/yyyyyyyyyy"; // <-- ¡Aquí va tu ID real!
        #else
            string adUnitId = "unexpected_platform";
        #endif

        // Creamos un banner de tamaño estándar en la parte inferior de la pantalla.
        this.bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);

        // Creamos una solicitud de anuncio.
        // Creamos una solicitud de anuncio.
        AdRequest request = new AdRequest();
        // Cargamos el banner con la solicitud.
        this.bannerView.LoadAd(request);
    }

    // Es una buena práctica destruir el banner cuando el objeto se destruye
    // para liberar memoria.
    private void OnDestroy()
    {
        if (this.bannerView != null)
        {
            this.bannerView.Destroy();
        }
    }
}