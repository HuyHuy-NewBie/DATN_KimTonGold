import 'package:flutter/material.dart';

import '../app_state.dart';
import '../formatters.dart';
import '../widgets.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key, required this.state});

  final AppState state;

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  @override
  Widget build(BuildContext context) {
    final user = widget.state.user!;
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        Row(
          children: [
            CircleAvatar(radius: 30, child: Text(_initial(user.fullName))),
            const SizedBox(width: 14),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    user.fullName,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: Theme.of(context).textTheme.titleLarge,
                  ),
                  Text(roleLabel(user.highestRole)),
                  Text(
                    user.branchName ?? 'Chưa gán chi nhánh',
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                ],
              ),
            ),
          ],
        ),
        const SectionTitle(title: 'Thông tin đăng nhập'),
        _InfoRow(label: 'Email', value: user.email ?? '--'),
        _InfoRow(label: 'Số điện thoại', value: user.phoneNumber ?? '--'),
        const SizedBox(height: 18),
        FilledButton.tonalIcon(
          onPressed: () => widget.state.bootstrap(),
          icon: const Icon(Icons.sync),
          label: const Text('Đồng bộ lại dữ liệu nền'),
        ),
        const SizedBox(height: 10),
        OutlinedButton.icon(
          onPressed: () => widget.state.logout(),
          icon: const Icon(Icons.logout),
          label: const Text('Đăng xuất'),
        ),
      ],
    );
  }

  String _initial(String value) {
    final text = value.trim();
    return text.isEmpty ? 'KT' : text.substring(0, 1).toUpperCase();
  }
}

class _InfoRow extends StatelessWidget {
  const _InfoRow({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 112,
            child: Text(
              label,
              style: TextStyle(
                color: Theme.of(context).colorScheme.onSurfaceVariant,
              ),
            ),
          ),
          Expanded(
            child: SelectableText(
              value,
              style: Theme.of(context).textTheme.bodyMedium,
            ),
          ),
        ],
      ),
    );
  }
}
