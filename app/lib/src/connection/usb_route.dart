import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart' show kIsWeb;

/// Conexão pelo cabo USB. Com a "Conexão USB" ligada no PC e a depuração USB no celular,
/// o PC faz o 127.0.0.1:4787 do celular cair nele (adb reverse). O app prefere esse
/// caminho quando ele existe: funciona sem Wi-Fi e tem menos atraso.
class UsbRoute {
  UsbRoute._();

  static const usbBaseUrl = 'http://127.0.0.1:4787';

  /// Último endereço de rede do PC, para voltar a ele quando o cabo sai.
  static String? _lanBaseUrl;

  /// A conexão atual é pelo cabo?
  static bool isUsb(String baseUrl) => baseUrl == usbBaseUrl;

  /// Endereço a usar: o do cabo, se o PC do outro lado aceita esta chave (assim um cabo
  /// ligado em outro PC não troca o PC pareado); senão o da rede.
  static Future<String> pick(String baseUrl, {String? apiKey}) async {
    if (kIsWeb) return baseUrl;
    final lan = isUsb(baseUrl) ? (_lanBaseUrl ?? baseUrl) : baseUrl;
    if (!isUsb(lan)) _lanBaseUrl = lan;
    if (apiKey == null || apiKey.isEmpty) return lan;
    try {
      await Dio(BaseOptions(
        connectTimeout: const Duration(milliseconds: 600),
        receiveTimeout: const Duration(seconds: 2),
        headers: {'X-API-Key': apiKey},
      )).get('$usbBaseUrl/api/StreamDeck/layout');
      return usbBaseUrl;
    } catch (_) {
      return lan;
    }
  }
}
