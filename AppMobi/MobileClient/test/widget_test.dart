import 'package:flutter_test/flutter_test.dart';
import 'package:shared_preferences/shared_preferences.dart';

import 'package:gold_mobile/main.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  testWidgets('shows login screen after initialization', (tester) async {
    SharedPreferences.setMockInitialValues({});

    await tester.pumpWidget(const GoldMobileApp());
    await tester.pumpAndSettle();

    expect(find.text('Kim Tôn Mobile'), findsOneWidget);
    expect(find.text('Đăng nhập'), findsOneWidget);
  });
}
