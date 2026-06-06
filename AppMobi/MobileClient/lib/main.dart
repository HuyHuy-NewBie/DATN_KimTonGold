import 'package:flutter/material.dart';

import 'app_state.dart';
import 'formatters.dart';
import 'screens/branches_screen.dart';
import 'screens/dashboard_screen.dart';
import 'screens/market_screen.dart';
import 'screens/orders_screen.dart';
import 'screens/products_screen.dart';
import 'screens/profile_screen.dart';
import 'screens/reports_screen.dart';
import 'screens/users_screen.dart';
import 'widgets.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  runApp(const GoldMobileApp());
}

class GoldMobileApp extends StatefulWidget {
  const GoldMobileApp({super.key});

  @override
  State<GoldMobileApp> createState() => _GoldMobileAppState();
}

class _GoldMobileAppState extends State<GoldMobileApp> {
  late final AppState state;

  @override
  void initState() {
    super.initState();
    state = AppState()..initialize();
  }

  @override
  Widget build(BuildContext context) {
    const seed = Color(0xFF7A5A20);
    return AnimatedBuilder(
      animation: state,
      builder: (context, _) {
        return MaterialApp(
          title: 'Kim Ton',
          debugShowCheckedModeBanner: false,
          theme: ThemeData(
            useMaterial3: true,
            colorScheme: ColorScheme.fromSeed(
              seedColor: seed,
              brightness: Brightness.light,
            ),
            scaffoldBackgroundColor: const Color(0xFFFAF8F3),
            inputDecorationTheme: const InputDecorationTheme(
              border: OutlineInputBorder(
                borderRadius: BorderRadius.all(Radius.circular(8)),
              ),
              filled: true,
              fillColor: Colors.white,
            ),
            cardTheme: CardThemeData(
              margin: EdgeInsets.zero,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(8),
              ),
            ),
          ),
          home: _home(),
        );
      },
    );
  }

  Widget _home() {
    if (!state.initialized) {
      return const Scaffold(body: LoadingPane(label: 'Đang chuẩn bị ứng dụng'));
    }
    if (!state.isLoggedIn) {
      return LoginScreen(state: state);
    }
    return AppShell(state: state);
  }
}

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key, required this.state});

  final AppState state;

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _identifierController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _remember = true;
  bool _obscure = true;

  @override
  void dispose() {
    _identifierController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final colors = Theme.of(context).colorScheme;
    return Scaffold(
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(20),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 460),
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    Icon(
                      Icons.diamond_outlined,
                      size: 58,
                      color: colors.primary,
                    ),
                    const SizedBox(height: 14),
                    Text(
                      'Kim Ton',
                      textAlign: TextAlign.center,
                      style: Theme.of(context).textTheme.headlineSmall
                          ?.copyWith(fontWeight: FontWeight.w700),
                    ),
                    const SizedBox(height: 24),
                    TextFormField(
                      controller: _identifierController,
                      decoration: const InputDecoration(
                        labelText: 'Email hoặc số điện thoại',
                        prefixIcon: Icon(Icons.person_outline),
                      ),
                      keyboardType: TextInputType.emailAddress,
                      validator: (value) =>
                          (value == null || value.trim().isEmpty)
                          ? 'Vui lòng nhập tài khoản'
                          : null,
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _passwordController,
                      obscureText: _obscure,
                      decoration: InputDecoration(
                        labelText: 'Mật khẩu',
                        prefixIcon: const Icon(Icons.lock_outline),
                        suffixIcon: IconButton(
                          tooltip: _obscure ? 'Hiện mật khẩu' : 'Ẩn mật khẩu',
                          onPressed: () => setState(() => _obscure = !_obscure),
                          icon: Icon(
                            _obscure
                                ? Icons.visibility_outlined
                                : Icons.visibility_off_outlined,
                          ),
                        ),
                      ),
                      validator: (value) => (value == null || value.isEmpty)
                          ? 'Vui lòng nhập mật khẩu'
                          : null,
                    ),
                    const SizedBox(height: 6),
                    SwitchListTile(
                      value: _remember,
                      onChanged: (value) => setState(() => _remember = value),
                      contentPadding: EdgeInsets.zero,
                      title: const Text('Ghi nhớ đăng nhập?'),
                    ),
                    const SizedBox(height: 12),
                    FilledButton.icon(
                      onPressed: widget.state.busy ? null : _submit,
                      icon: widget.state.busy
                          ? const SizedBox(
                              width: 18,
                              height: 18,
                              child: AnimatedLogoLoader(size: 18),
                            )
                          : const Icon(Icons.login),
                      label: const Text('Đăng nhập'),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }
    try {
      await widget.state.login(
        apiBase: widget.state.apiBase,
        identifier: _identifierController.text.trim(),
        password: _passwordController.text,
        rememberDevice: _remember,
      );
    } catch (error) {
      if (mounted) {
        showSnack(context, error.toString());
      }
    }
  }
}

class AppShell extends StatefulWidget {
  const AppShell({super.key, required this.state});

  final AppState state;

  @override
  State<AppShell> createState() => _AppShellState();
}

class _Destination {
  const _Destination({
    required this.title,
    required this.icon,
    required this.selectedIcon,
    required this.builder,
    this.permission,
  });

  final String title;
  final IconData icon;
  final IconData selectedIcon;
  final String? permission;
  final Widget Function(AppState state) builder;
}

class _AppShellState extends State<AppShell> {
  int selectedIndex = 0;

  List<_Destination> get _destinations {
    final user = widget.state.user;
    bool can(String permission) => user?.can(permission) == true;
    return [
      _Destination(
        title: 'Tổng quan',
        icon: Icons.dashboard_outlined,
        selectedIcon: Icons.dashboard,
        builder: (state) => DashboardScreen(state: state),
      ),
      _Destination(
        title: 'Tỷ giá',
        icon: Icons.show_chart_outlined,
        selectedIcon: Icons.show_chart,
        builder: (state) => MarketScreen(state: state),
      ),
      if (can('products:read'))
        _Destination(
          title: 'Sản phẩm',
          icon: Icons.inventory_2_outlined,
          selectedIcon: Icons.inventory_2,
          permission: 'products:read',
          builder: (state) => ProductsScreen(state: state),
        ),
      if (can('orders:read'))
        _Destination(
          title: 'Đơn hàng',
          icon: Icons.receipt_long_outlined,
          selectedIcon: Icons.receipt_long,
          permission: 'orders:read',
          builder: (state) => OrdersScreen(state: state),
        ),
      if (can('reports:read'))
        _Destination(
          title: 'Báo cáo',
          icon: Icons.bar_chart_outlined,
          selectedIcon: Icons.bar_chart,
          permission: 'reports:read',
          builder: (state) => ReportsScreen(state: state),
        ),
      if (can('branches:read'))
        _Destination(
          title: 'Chi nhánh',
          icon: Icons.store_mall_directory_outlined,
          selectedIcon: Icons.store_mall_directory,
          permission: 'branches:read',
          builder: (state) => BranchesScreen(state: state),
        ),
      if (can('users:manage'))
        _Destination(
          title: 'Nhân sự',
          icon: Icons.group_outlined,
          selectedIcon: Icons.group,
          permission: 'users:manage',
          builder: (state) => UsersScreen(state: state),
        ),
      _Destination(
        title: 'Tài khoản',
        icon: Icons.account_circle_outlined,
        selectedIcon: Icons.account_circle,
        builder: (state) => ProfileScreen(state: state),
      ),
    ];
  }

  @override
  Widget build(BuildContext context) {
    final destinations = _destinations;
    if (selectedIndex >= destinations.length) {
      selectedIndex = 0;
    }
    final selected = destinations[selectedIndex];
    final user = widget.state.user!;

    return Stack(
      children: [
        Scaffold(
          appBar: AppBar(
            title: Text(selected.title),
            actions: [
              IconButton(
                tooltip: 'Làm mới',
                onPressed: () => widget.state.refresh(),
                icon: const Icon(Icons.refresh),
              ),
            ],
          ),
          drawer: NavigationDrawer(
            selectedIndex: selectedIndex,
            onDestinationSelected: (index) {
              setState(() => selectedIndex = index);
              Navigator.pop(context);
            },
            children: [
              Padding(
                padding: const EdgeInsets.fromLTRB(20, 18, 20, 10),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    CircleAvatar(
                      radius: 26,
                      child: Text(
                        user.fullName.isEmpty
                            ? 'KT'
                            : user.fullName.characters.first.toUpperCase(),
                      ),
                    ),
                    const SizedBox(height: 10),
                    Text(
                      user.fullName,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                    Text(
                      roleLabel(user.highestRole),
                      style: TextStyle(
                        color: Theme.of(context).colorScheme.onSurfaceVariant,
                      ),
                    ),
                    if (user.branchName != null)
                      Text(
                        user.branchName!,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                      ),
                  ],
                ),
              ),
              const Divider(),
              for (final destination in destinations)
                NavigationDrawerDestination(
                  icon: Icon(destination.icon),
                  selectedIcon: Icon(destination.selectedIcon),
                  label: Text(destination.title),
                ),
              const Divider(),
              Padding(
                padding: const EdgeInsets.all(12),
                child: OutlinedButton.icon(
                  onPressed: () => widget.state.logout(),
                  icon: const Icon(Icons.logout),
                  label: const Text('Đăng xuất'),
                ),
              ),
            ],
          ),
          body: SafeArea(child: selected.builder(widget.state)),
        ),
        if (widget.state.globalLoading)
          Positioned.fill(
            child: Container(
              color: Colors.black.withValues(alpha: 0.3),
              child: const Center(
                child: Card(
                  child: Padding(
                    padding: EdgeInsets.all(24),
                    child: LoadingPane(label: 'Đang làm mới dữ liệu...'),
                  ),
                ),
              ),
            ),
          ),
      ],
    );
  }
}

