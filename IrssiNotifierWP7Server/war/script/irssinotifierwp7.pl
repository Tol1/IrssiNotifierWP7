#use strict;
#use warnings;

use Irssi;
use POSIX;
use Encode;
require CGI::Util;
use vars qw($VERSION %IRSSI);

$VERSION = "2";
%IRSSI = (
	authors		=> "Tomi \'Tol1\' Nokkala, Lauri \'murgo\' Härsilä",
	contact		=> "tol1\@iki.fi",
	name		=> "Irssi Notifier for Windows Phone",
	description	=> "Send notifications about Irssi highlights to server",
	license		=> "Apache License, version 2.0",
	url			=> "http://irssinotifierwp.appspot.com",
	changed		=> "2013-05-18"
);

my $settingsfile = Irssi::get_irssi_dir."/irssinotifierwp.conf";

my @ignorePatterns = ();

my $lastMsg;
my $lastServer;
my $lastNick;
my $lastAddress;
my $lastTarget;
my $lastKeyboardActivity = time;

my $in_progress = 0;
my @messageQueue;

my $screen_socket_path;

sub private {
	my ($server, $msg, $nick, $address) = @_;
	$lastMsg = $msg;
	$lastServer = $server;
	$lastNick = $nick;
	$lastAddress = $address;
	$lastTarget = "PRIVATE";
}

sub public {
	my ($server, $msg, $nick, $address, $target) = @_;
	$lastMsg = $msg;
	$lastServer = $server;
	$lastNick = $nick;
	$lastAddress = $address;
	$lastTarget = $target;
}

sub screen_attached {
    if (!$screen_socket_path || !defined($ENV{STY})) {
        return 1;
    }
    my $socket = $screen_socket_path . "/" . $ENV{'STY'};
    if (-e $socket && ((stat($socket))[2] & 00100) != 0) {
        return 1;
    }
    return 0;
}

sub print_text {
	my ($dest, $text, $stripped) = @_;

	my $opt = MSGLEVEL_HILIGHT|MSGLEVEL_MSGS;
	if (should_hilite($dest)) {
		hilite();
	}
}

sub should_hilite {
	my $target = @_ ? shift : $_;

	my $opt = MSGLEVEL_HILIGHT | MSGLEVEL_MSGS;
	if (!($target->{level} & $opt) || ($target->{level} & MSGLEVEL_NOHILIGHT)) {
		return 0;
	}
	if(Irssi::settings_get_bool("irssinotifierwp_away_only") && !$lastServer->{usermode_away}) {
		return 0;
	}
	if(Irssi::settings_get_bool("irssinotifierwp_ignore_active_window") && ($target->{window}->{refnum} == (Irssi::active_win()->{refnum}))) {
		return 0;
	}
	if(!activity_allows_hilight()) {
		return 0;
	}
	if(Irssi::settings_get_bool("irssinotifierwp_screen_detached_only") && screen_attached())  {
		return 0;
	}
	my @ignored_channels = split(/ /, Irssi::settings_get_str("irssinotifierwp_ignore_channels"));
	my $channel;

	foreach $channel (@ignored_channels) {
		if (lc($lastTarget) eq lc($channel)) {
			return 0;
		}
	}
	if (grep { $lastMsg =~ /$_/i } @ignorePatterns) {
		return 0;
	}
	return 1;
}

sub activity_allows_hilight {
	my $timeout = Irssi::settings_get_int('irssinotifierwp_require_idle_seconds');
	return ($timeout <= 0 || (time - $lastKeyboardActivity) > $timeout);
}

sub dangerous_string {
	my $s = @_ ? shift : $_;
	return $s =~ m/"/ || $s =~ m/`/ || $s =~ m/\\/;
}

sub hilite {
	if (!Irssi::settings_get_str('irssinotifierwp_api_token')) {
		print_info("Set API token to send notifications: /set irssinotifierwp_api_token [token]");
		return;
	}

	`/usr/bin/env wget --version`;
	if ($? != 0) {
		print_info("You'll need to install Wget to use Irssi Notifier");
		return;
	}
	my $encoding;
	eval {
		require I18N::Langinfo;
		$encoding = lc(I18N::Langinfo::langinfo(I18N::Langinfo::CODESET()));
	};
	if ($encoding && $encoding !~ /^utf-?8$/i) {
		$lastMsg    = Encode::encode_utf8($lastMsg);
		$lastNick   = Encode::encode_utf8($lastNick);
		$lastTarget = Encode::encode_utf8($lastTarget);
	}
	
	$lastMsg = CGI::Util::escape($lastMsg);
	$lastNick = CGI::Util::escape($lastNick);
	$lastTarget = CGI::Util::escape($lastTarget);
	
	if($in_progress){ # Jonoon
		my @message = ($lastMsg, $lastNick, $lastTarget);
		unshift(@messageQueue, \@message);
		return;
	}
	else{
		pipe_and_fork($lastMsg, $lastNick, $lastTarget);
	}
}

sub pipe_and_fork {
	my ($message, $nick, $target) = @_;
	
	my $api_token = Irssi::settings_get_str('irssinotifierwp_api_token');
	if (dangerous_string $api_token) {
		print_info("Api token cannot contain backticks, double quotes or backslashes");
		return;
	}
	
	pipe(my $read_handle, my $write_handle);
	
	my $pid = fork();
	if (!defined($pid)) {
		print_info("Can't fork() - aborting");
		close($read_handle);
		close($write_handle);
		return;
	}
	
	$in_progress = 1;
	
	if ($pid > 0) { # parent
		close ($write_handle);
		Irssi::pidwait_add($pid);
		my $job = $pid;
		my $tag;
		my @args = ($read_handle, \$tag, $job);
		$tag = Irssi::input_add(fileno($read_handle), Irssi::INPUT_READ, \&pipe_input, \@args);
	}
	else { # child
		my $data = "--post-data \"apiToken=$api_token\&message=$message\&channel=$target\&nick=$nick\&version=$VERSION\"";
		
		my $result = `wget --no-check-certificate --user-agent="irssinotifierWP7script/$VERSION" -qO- /dev/null $data https://irssinotifierwp.appspot.com/irssi/message`;
		if ($? != 0) {
			# Something went wrong, might be network error or authorization issue. Probably no need to alert user, though.
			# Irssi::print("Irssi Notifier: Sending hilight to server failed, check http://irssinotifierwp.appspot.com for updates");
			# return;
		}
		else{
			if (length($result) > 0) {
				print $write_handle $result;
			}
		#	else{
		#		print $write_handle "No problems";
		#	}
		}
		
		close($write_handle);
		
		POSIX::_exit(1);
	}
}

sub pipe_input {
	my $args = shift;
	my ($read_handle, $pipetag, $job) = @$args;
	my $line = <$read_handle>;
	close($read_handle);
	Irssi::input_remove($$pipetag);
	if(length($line) > 0){
		print_info("$line");
	}
	$in_progress = 0;
	if(@messageQueue > 0){
		$next = pop(@messageQueue);
		pipe_and_fork($next->[0], $next->[1], $next->[2]);
	}
}

sub setup_keypress_handler {
	Irssi::signal_remove('gui key pressed', 'event_key_pressed');
	if (Irssi::settings_get_int('irssinotifierwp_require_idle_seconds') > 0) {
		Irssi::signal_add('gui key pressed', 'event_key_pressed');
	}
}

sub event_key_pressed {
	$lastKeyboardActivity = time;
}

my $screen_ls = `LC_ALL="C" screen -ls`;
if ($screen_ls !~ /^No Sockets found/s) {
    $screen_ls =~ /^.+\d+ Sockets? in ([^\n]+)\.\n.+$/s;
    $screen_socket_path = $1;
} else {
    $screen_ls =~ /^No Sockets found in ([^\n]+)\.\n.+$/s;
    $screen_socket_path = $1;
}

sub cmd_add_ignore_channel {
	my ($channel) = @_;
	my @ignored_channels = split(/ /, Irssi::settings_get_str("irssinotifierwp_ignore_channels"));
	push(@ignored_channels, trim(lc($channel)));
	Irssi::settings_set_str("irssinotifierwp_ignore_channels", join(" ", @ignored_channels));
	print_info("Added ingored channel: $channel");
}

sub cmd_delete_ignore_channel {
	my ($removedchannel) = @_;
	$removedchannel = trim(lc($removedchannel));	#Trim. To be sure. Stupid perl.
	my @ignored_channels = split(/ /, Irssi::settings_get_str("irssinotifierwp_ignore_channels"));
	@ignored_channels = grep {$_ ne $removedchannel} @ignored_channels;
	Irssi::settings_set_str("irssinotifierwp_ignore_channels", join(" ", @ignored_channels));
	print_info("Removed ingored channel: $removedchannel");
}

sub sig_complete_channel {
	my ($strings, $window, $word, $linestart, $want_space) = @_;
	return unless ($linestart eq '/irssinotifierwp_delete_ignore_channel');
	my @ignored_channels = split(/ /, Irssi::settings_get_str("irssinotifierwp_ignore_channels"));
	$search = quotemeta "$word";
	@$strings = grep(/^$search/, @ignored_channels);
	$$want_space = 0;
	Irssi::signal_stop();
}

sub cmd_add_ignore_pattern {
	my ($pattern) = @_;
	push(@ignorePatterns, $pattern);
	save_to_settings_file();
}

sub cmd_list_ignore_patterns {
	for my $i (0 .. $#ignorePatterns) {
		print_info("[".($i + 1)."]: $ignorePatterns[$i]");
	}
}

sub cmd_delete_ignore_pattern {
	my ($index) = @_;
	if ($index =~ /^[0-9]*$/ and defined($ignorePatterns[$index-1])) {
		splice(@ignorePatterns, ($index-1), 1);
		save_to_settings_file();
	}
	else {
		print_info("Bad index");
		return 0;
	}
}

sub trim {
	my $string = shift;
	$string =~ s/^\s+//;
	$string =~ s/\s+$//;
	return $string;
}

sub print_info {
	Irssi::printformat(MSGLEVEL_CLIENTCRAP, 'irssinotifier', join(" ", @_));
}

sub load_from_settings_file {
	my $ret = open(SETTINGSFILE, $settingsfile);

	if (!$ret) {
		if (-e $settingsfile) {
			print_info("Couldn't open the settings file \"$settingsfile\" for reading.");
		}
		return;
	}
	
	local $/ = "\n";
	my $line;
	
	while($line = <SETTINGSFILE>) {
		chomp $line;
		my @linedata = split(/\t/, $line, 2);
		
		if($linedata[0] eq 'ignore_pattern') {
			push(@ignorePatterns, $linedata[1]);
		}
	}
	
	close(SETTINGSFILE);
}

sub save_to_settings_file {
	my $ret = open (SETTINGSFILE, '>'.$settingsfile);
	
	if (!$ret) {
		print_info("Couldn't open the settings file \"$settingsfile\" for writing.");
		return;
	}
	
	foreach my $pattern (@ignorePatterns) {
		print SETTINGSFILE "ignore_pattern\t$pattern\n";
	}
	
	close(SETTINGSFILE);
	print_info("Settings saved successfully.") if (!$quiet);
}

Irssi::settings_add_str('IrssiNotifierWP', 'irssinotifierwp_api_token', '');
Irssi::settings_add_bool('IrssiNotifierWP', 'irssinotifierwp_away_only', false);
Irssi::settings_add_bool('IrssiNotifierWP', 'irssinotifierwp_ignore_active_window', false);
Irssi::settings_add_int('IrssiNotifierWP', 'irssinotifierwp_require_idle_seconds', 0);
Irssi::settings_add_bool('IrssinotifierWP', 'irssinotifierwp_screen_detached_only', 1);
Irssi::settings_add_str('IrssiNotifierWP', 'irssinotifierwp_ignore_channels', '');

Irssi::signal_add('message irc action', 'public');
Irssi::signal_add('message public', 'public');
Irssi::signal_add('message private', 'private');
Irssi::signal_add('print text', 'print_text');
Irssi::signal_add('setup changed', 'setup_keypress_handler');

Irssi::command_bind('irssinotifierwp_add_ignore_channel', 'cmd_add_ignore_channel');
Irssi::command_bind('irssinotifierwp_delete_ignore_channel', 'cmd_delete_ignore_channel');

Irssi::command_bind('irssinotifierwp_add_ignore_pattern', 'cmd_add_ignore_pattern');
Irssi::command_bind('irssinotifierwp_list_ignore_patterns', 'cmd_list_ignore_patterns');
Irssi::command_bind('irssinotifierwp_delete_ignore_pattern', 'cmd_delete_ignore_pattern');

Irssi::signal_add_first('complete word', 'sig_complete_channel');

Irssi::theme_register([
	'irssinotifier',
	'{line_start}{hilight Irssi Notifier:} $0',
]);

setup_keypress_handler();
load_from_settings_file();