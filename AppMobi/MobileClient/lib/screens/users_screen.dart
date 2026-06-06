import 'package:flutter/material.dart';

import '../app_state.dart';
import '../formatters.dart';
import '../models.dart';
import '../widgets.dart';

class UsersScreen extends StatefulWidget {
  const UsersScreen({super.key, required this.state});

  final AppState state;

  @override
  State<UsersScreen> createState() => _UsersScreenState();
}

class _UsersScreenState extends State<UsersScreen> {
  final search = TextEditingController();
  List<UserProfile> users = const [];
  bool loading = true;
  String? error;
  String? role;
  int? branchId;

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    search.dispose();
    super.dispose();
  }

  Future<void> _load() async {
    setState(() {
      loading = true;
      error = null;
    });
    try {
      final data = await widget.state.api.users(
        search: search.text.trim(),
        role: role,
        branchId: branchId,
      );
      setState(() => users = data);
    } catch (exception) {
      setState(() => error = exception.toString());
    } finally {
      if (mounted) {
        setState(() => loading = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.transparent,
      floatingActionButton: FloatingActionButton.extended(
        onPressed: _openCreateSheet,
        icon: const Icon(Icons.person_add_alt_1_outlined),
        label: const Text('Tạo'),
      ),
      body: RefreshIndicator(
        onRefresh: _load,
        child: ListView(
          padding: const EdgeInsets.fromLTRB(16, 16, 16, 88),
          children: [
            TextField(
              controller: search,
              decoration: InputDecoration(
                labelText: 'Tìm nhân sự',
                prefixIcon: const Icon(Icons.search),
                suffixIcon: IconButton(
                  tooltip: 'Tìm',
                  onPressed: _load,
                  icon: const Icon(Icons.arrow_forward),
                ),
              ),
              onSubmitted: (_) => _load(),
            ),
            const SizedBox(height: 10),
            Row(
              children: [
                Expanded(
                  child: DropdownButtonFormField<String?>(
                    initialValue: role,
                    decoration: const InputDecoration(labelText: 'Vai trò'),
                    items: [
                      const DropdownMenuItem(
                        value: null,
                        child: Text('Tất cả vai trò'),
                      ),
                      ...widget.state.metadata.roles.map(
                        (item) => DropdownMenuItem(
                          value: item,
                          child: Text(roleLabel(item)),
                        ),
                      ),
                    ],
                    onChanged: (value) {
                      setState(() => role = value);
                      _load();
                    },
                  ),
                ),
                const SizedBox(width: 10),
                Expanded(
                  child: BranchDropdown(
                    branches: widget.state.branches,
                    value: branchId,
                    onChanged: (value) {
                      setState(() => branchId = value);
                      _load();
                    },
                  ),
                ),
              ],
            ),
            const SizedBox(height: 12),
            if (loading)
              const SizedBox(height: 360, child: LoadingPane())
            else if (error != null)
              EmptyState(
                icon: Icons.error_outline,
                title: 'Không tải được nhân sự',
                message: error,
              )
            else if (users.isEmpty)
              const EmptyState(
                icon: Icons.group_outlined,
                title: 'Không có tài khoản',
              )
            else
              ...users.map(
                (user) => _UserTile(
                  user: user,
                  state: widget.state,
                  onChanged: _load,
                ),
              ),
          ],
        ),
      ),
    );
  }

  Future<void> _openCreateSheet() async {
    final saved = await showModalBottomSheet<bool>(
      context: context,
      isScrollControlled: true,
      showDragHandle: true,
      builder: (context) => _CreateUserSheet(state: widget.state),
    );
    if (saved == true) {
      await _load();
    }
  }
}

class _UserTile extends StatefulWidget {
  const _UserTile({
    required this.user,
    required this.state,
    required this.onChanged,
  });

  final UserProfile user;
  final AppState state;
  final Future<void> Function() onChanged;

  @override
  State<_UserTile> createState() => _UserTileState();
}

class _UserTileState extends State<_UserTile> {
  bool saving = false;

  @override
  Widget build(BuildContext context) {
    final isSelf = widget.user.id == widget.state.user?.id;
    return Card(
      margin: const EdgeInsets.only(bottom: 10),
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                CircleAvatar(child: Text(_initials(widget.user.fullName))),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        widget.user.fullName,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                        style: Theme.of(context).textTheme.titleMedium,
                      ),
                      Text(
                        widget.user.email ?? '--',
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                      ),
                      Text(
                        '${roleLabel(widget.user.highestRole)} • ${widget.user.branchName ?? 'Chưa gán chi nhánh'}',
                      ),
                    ],
                  ),
                ),
                Switch(
                  value: widget.user.isActive,
                  onChanged: isSelf || saving
                      ? null
                      : (value) => _toggleStatus(value),
                ),
              ],
            ),
            const SizedBox(height: 8),
            Row(
              children: [
                Expanded(
                  child: DropdownButtonFormField<String>(
                    initialValue: widget.user.highestRole,
                    decoration: const InputDecoration(labelText: 'Vai trò'),
                    items: widget.state.metadata.roles
                        .map(
                          (item) => DropdownMenuItem(
                            value: item,
                            child: Text(roleLabel(item)),
                          ),
                        )
                        .toList(),
                    onChanged: isSelf || saving
                        ? null
                        : (value) => value == null ? null : _updateRole(value),
                  ),
                ),
                if (saving) ...[
                  const SizedBox(width: 12),
                  const SizedBox(
                    width: 22,
                    height: 22,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  ),
                ],
              ],
            ),
          ],
        ),
      ),
    );
  }

  String _initials(String value) {
    final parts = value
        .trim()
        .split(RegExp(r'\s+'))
        .where((item) => item.isNotEmpty)
        .toList();
    if (parts.isEmpty) {
      return 'KT';
    }
    if (parts.length == 1) {
      return parts.first.substring(0, 1).toUpperCase();
    }
    return '${parts.first.substring(0, 1)}${parts.last.substring(0, 1)}'
        .toUpperCase();
  }

  Future<void> _toggleStatus(bool active) async {
    setState(() => saving = true);
    try {
      await widget.state.api.updateUserStatus(widget.user.id, active);
      if (mounted) {
        showSnack(
          context,
          active ? 'Đã mở khóa tài khoản' : 'Đã khóa tài khoản',
        );
      }
      await widget.onChanged();
    } catch (exception) {
      if (mounted) {
        showSnack(context, exception.toString());
      }
    } finally {
      if (mounted) {
        setState(() => saving = false);
      }
    }
  }

  Future<void> _updateRole(String role) async {
    setState(() => saving = true);
    try {
      await widget.state.api.updateUserRole(widget.user.id, role);
      if (mounted) {
        showSnack(context, 'Đã cập nhật vai trò');
      }
      await widget.onChanged();
    } catch (exception) {
      if (mounted) {
        showSnack(context, exception.toString());
      }
    } finally {
      if (mounted) {
        setState(() => saving = false);
      }
    }
  }
}

class _CreateUserSheet extends StatefulWidget {
  const _CreateUserSheet({required this.state});

  final AppState state;

  @override
  State<_CreateUserSheet> createState() => _CreateUserSheetState();
}

class _CreateUserSheetState extends State<_CreateUserSheet> {
  final _formKey = GlobalKey<FormState>();
  final fullName = TextEditingController();
  final email = TextEditingController();
  final password = TextEditingController();
  late String role = widget.state.metadata.roles.contains('Staff')
      ? 'Staff'
      : widget.state.metadata.roles.first;
  int? branchId;
  bool saving = false;
  bool obscure = true;

  @override
  void initState() {
    super.initState();
    branchId =
        widget.state.user?.branchId ?? widget.state.branches.firstOrNull?.id;
  }

  @override
  void dispose() {
    fullName.dispose();
    email.dispose();
    password.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final isAdmin = widget.state.user?.hasRole('Admin') == true;
    return Padding(
      padding: EdgeInsets.only(bottom: MediaQuery.viewInsetsOf(context).bottom),
      child: SafeArea(
        top: false,
        child: Form(
          key: _formKey,
          child: ListView(
            padding: const EdgeInsets.all(16),
            shrinkWrap: true,
            children: [
              Text(
                'Tạo tài khoản',
                style: Theme.of(context).textTheme.titleLarge,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: fullName,
                decoration: const InputDecoration(labelText: 'Họ tên'),
                validator: _required,
              ),
              const SizedBox(height: 10),
              TextFormField(
                controller: email,
                decoration: const InputDecoration(labelText: 'Email'),
                keyboardType: TextInputType.emailAddress,
                validator: _required,
              ),
              const SizedBox(height: 10),
              TextFormField(
                controller: password,
                obscureText: obscure,
                decoration: InputDecoration(
                  labelText: 'Mật khẩu khởi tạo',
                  suffixIcon: IconButton(
                    tooltip: obscure ? 'Hiện mật khẩu' : 'Ẩn mật khẩu',
                    onPressed: () => setState(() => obscure = !obscure),
                    icon: Icon(
                      obscure
                          ? Icons.visibility_outlined
                          : Icons.visibility_off_outlined,
                    ),
                  ),
                ),
                validator: (value) => value == null || value.length < 6
                    ? 'Mật khẩu tối thiểu 6 ký tự'
                    : null,
              ),
              const SizedBox(height: 10),
              DropdownButtonFormField<String>(
                initialValue: role,
                decoration: const InputDecoration(labelText: 'Vai trò'),
                items: widget.state.metadata.roles
                    .map(
                      (item) => DropdownMenuItem(
                        value: item,
                        child: Text(roleLabel(item)),
                      ),
                    )
                    .toList(),
                onChanged: (value) => setState(() => role = value ?? role),
              ),
              const SizedBox(height: 10),
              BranchDropdown(
                branches: widget.state.branches,
                value: branchId,
                includeAll: isAdmin,
                onChanged: isAdmin
                    ? (value) => setState(() => branchId = value)
                    : (_) {},
              ),
              const SizedBox(height: 16),
              FilledButton.icon(
                onPressed: saving ? null : _save,
                icon: saving
                    ? const SizedBox(
                        width: 18,
                        height: 18,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Icon(Icons.save_outlined),
                label: const Text('Tạo tài khoản'),
              ),
            ],
          ),
        ),
      ),
    );
  }

  String? _required(String? value) {
    return value == null || value.trim().isEmpty ? 'Không được để trống' : null;
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }
    setState(() => saving = true);
    try {
      await widget.state.api.createUser(
        fullName: fullName.text.trim(),
        email: email.text.trim(),
        password: password.text,
        role: role,
        branchId: branchId,
      );
      if (mounted) {
        showSnack(context, 'Đã tạo tài khoản');
        Navigator.pop(context, true);
      }
    } catch (exception) {
      if (mounted) {
        showSnack(context, exception.toString());
      }
    } finally {
      if (mounted) {
        setState(() => saving = false);
      }
    }
  }
}
