import 'dart:ui' show PlatformDispatcher;

import 'package:shared_preferences/shared_preferences.dart';
import 'package:signals_flutter/signals_flutter.dart';

import 'en/en.dart';
import 'es/es.dart';
import 'hi/hi.dart';

/// Idioma da interface: o texto-fonte é português. Traduções ausentes em
/// espanhol ou hindi recorrem ao inglês; texto sem tradução aparece em português.
class AppLanguage {
  AppLanguage._();

  static const _prefKey = 'app_language';

  /// Escolha do usuário: 'auto' (idioma do aparelho), 'pt', 'en', 'es' ou 'hi'.
  static final choice = signal('auto');

  /// Idioma em uso.
  static final current = computed(() => resolve(choice.value));

  static String resolve(String choice) {
    if (choice == 'pt' || choice == 'en' || choice == 'es' || choice == 'hi') {
      return choice;
    }
    final device = PlatformDispatcher.instance.locale.languageCode;
    if (device == 'pt' || device == 'es' || device == 'hi') return device;
    return 'en';
  }

  static Future<void> load() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      choice.value = prefs.getString(_prefKey) ?? 'auto';
    } catch (_) {}
  }

  static Future<void> set(String value) async {
    choice.value = value;
    try {
      await (await SharedPreferences.getInstance()).setString(_prefKey, value);
    } catch (_) {}
  }

  /// Valor do cabeçalho Accept-Language enviado ao PC.
  static String get header => switch (current.value) {
        'en' => 'en',
        'es' => 'es',
        'hi' => 'hi-IN',
        _ => 'pt-BR',
      };
}

String tr(String pt, [Map<String, Object?> args = const {}]) {
  var s = switch (AppLanguage.current.value) {
    'en' => enStrings[pt] ?? pt,
    'es' => esStrings[pt] ?? enStrings[pt] ?? pt,
    'hi' => hiStrings[pt] ?? enStrings[pt] ?? pt,
    _ => pt,
  };
  args.forEach((k, v) => s = s.replaceAll('{$k}', '${v ?? ''}'));
  return s;
}
