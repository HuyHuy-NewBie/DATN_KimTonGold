import 'package:flutter/material.dart';

import '../app_state.dart';
import '../models.dart';
import '../widgets.dart';

class BranchesScreen extends StatefulWidget {
  const BranchesScreen({super.key, required this.state});

  final AppState state;

  @override
  State<BranchesScreen> createState() => _BranchesScreenState();
}

class _BranchesScreenState extends State<BranchesScreen> {
  bool loading = false;

  @override
  Widget build(BuildContext context) {
    final canManage = widget.state.user?.can('branches:manage') == true;
    final branches = widget.state.branches;
    return Scaffold(
      backgroundColor: Colors.transparent,
      floatingActionButton: canManage
          ? FloatingActionButton.extended(
              onPressed: _openCreateSheet,
              icon: const Icon(Icons.add_business_outlined),
              label: const Text('Thêm'),
            )
          : null,
      body: RefreshIndicator(
        onRefresh: _reload,
        child: ListView(
          padding: const EdgeInsets.fromLTRB(16, 16, 16, 88),
          children: [
            if (loading) const LinearProgressIndicator(),
            if (branches.isEmpty)
              const SizedBox(
                height: 360,
                child: EmptyState(
                  icon: Icons.store_mall_directory_outlined,
                  title: 'Không có chi nhánh',
                ),
              )
            else
              ...branches.map(
                (branch) => _BranchTile(
                  branch: branch,
                  canManage: canManage,
                  onToggle: () => _toggle(branch),
                ),
              ),
          ],
        ),
      ),
    );
  }

  Future<void> _reload() async {
    setState(() => loading = true);
    try {
      await widget.state.refreshBranches();
    } catch (exception) {
      if (mounted) {
        showSnack(context, exception.toString());
      }
    } finally {
      if (mounted) {
        setState(() => loading = false);
      }
    }
  }

  Future<void> _toggle(Branch branch) async {
    final next = !branch.isActive;
    final confirmed = await confirmAction(
      context,
      title: next ? 'Mở khóa chi nhánh' : 'Khóa chi nhánh',
      message: next
          ? 'Kích hoạt lại ${branch.branchName}?'
          : 'Tạm khóa ${branch.branchName}?',
    );
    if (!confirmed) {
      return;
    }
    try {
      await widget.state.api.updateBranchStatus(branch.id, next);
      await _reload();
      if (mounted) {
        showSnack(context, next ? 'Đã mở khóa chi nhánh' : 'Đã khóa chi nhánh');
      }
    } catch (exception) {
      if (mounted) {
        showSnack(context, exception.toString());
      }
    }
  }

  Future<void> _openCreateSheet() async {
    final saved = await showModalBottomSheet<bool>(
      context: context,
      isScrollControlled: true,
      showDragHandle: true,
      builder: (context) => _CreateBranchSheet(state: widget.state),
    );
    if (saved == true) {
      await _reload();
    }
  }
}

class _BranchTile extends StatelessWidget {
  const _BranchTile({
    required this.branch,
    required this.canManage,
    required this.onToggle,
  });

  final Branch branch;
  final bool canManage;
  final VoidCallback onToggle;

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 10),
      child: ListTile(
        leading: CircleAvatar(
          child: Icon(
            branch.isActive ? Icons.storefront_outlined : Icons.storefront,
          ),
        ),
        title: Text(
          branch.branchName,
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
        ),
        subtitle: Text(
          [
            if (branch.address != null) branch.address!,
            if (branch.phoneNumber != null) branch.phoneNumber!,
          ].join(' • '),
        ),
        trailing: canManage
            ? Switch(value: branch.isActive, onChanged: (_) => onToggle())
            : StatusChip(branch.isActive ? 'Đang hoạt động' : 'Tạm khóa'),
      ),
    );
  }
}

class _CreateBranchSheet extends StatefulWidget {
  const _CreateBranchSheet({required this.state});

  final AppState state;

  @override
  State<_CreateBranchSheet> createState() => _CreateBranchSheetState();
}

class _CreateBranchSheetState extends State<_CreateBranchSheet> {
  final _formKey = GlobalKey<FormState>();
  final name = TextEditingController();
  final address = TextEditingController();
  final phone = TextEditingController();
  bool saving = false;

  @override
  void dispose() {
    name.dispose();
    address.dispose();
    phone.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
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
                'Thêm chi nhánh',
                style: Theme.of(context).textTheme.titleLarge,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: name,
                decoration: const InputDecoration(labelText: 'Tên chi nhánh'),
                validator: (value) => value == null || value.trim().isEmpty
                    ? 'Vui lòng nhập tên chi nhánh'
                    : null,
              ),
              const SizedBox(height: 10),
              TextFormField(
                controller: address,
                decoration: const InputDecoration(labelText: 'Địa chỉ'),
              ),
              const SizedBox(height: 10),
              TextFormField(
                controller: phone,
                decoration: const InputDecoration(labelText: 'Số điện thoại'),
                keyboardType: TextInputType.phone,
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
                label: const Text('Lưu chi nhánh'),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }
    setState(() => saving = true);
    try {
      await widget.state.api.createBranch(
        branchName: name.text.trim(),
        address: address.text.trim().isEmpty ? null : address.text.trim(),
        phoneNumber: phone.text.trim().isEmpty ? null : phone.text.trim(),
      );
      if (mounted) {
        showSnack(context, 'Đã thêm chi nhánh');
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
