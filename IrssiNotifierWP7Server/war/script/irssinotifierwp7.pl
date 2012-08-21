#use strict;
#use warnings;

use Irssi;
use POSIX;
require CGI::Util;
use vars qw($VERSION %IRSSI);

$VERSION = "1";
%IRSSI = (
	authors		=> "Tomi \'Tol1\' Nokkala, Lauri \'murgo\' Härsilä",
	contact		=> "tol1\@iki.fi",
	name		=> "IrssiNotifier for Windows Phone",
	description	=> "Send notifications about irssi highlights to server",
	license		=> "Apache License, version 2.0",
	url			=> "http://irssinotifierwp.appspot.com",
	changed		=> "2012-08-21"
);

my $lastMsg;
my $lastServer;
my $lastNick;
my $lastAddress;
my $lastTarget;
my $lastKeyboardActivity = time;

my $in_progress = 0;
my @messageQueue;

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

sub print_text {
	my ($dest, $text, $stripped) = @_;

	my $opt = MSGLEVEL_HILIGHT|MSGLEVEL_MSGS;
	if (
		($dest->{level} & ($opt)) && (($dest->{level} & MSGLEVEL_NOHILIGHT) == 0) &&
		(!Irssi::settings_get_bool("irssinotifierwp_away_only") || $lastServer->{usermode_away}) &&
		(!Irssi::settings_get_bool("irssinotifierwp_ignore_active_window") || ($dest->{window}->{refnum} != (Irssi::active_win()->{refnum}))) &&
		activity_allows_hilight()
	) {
		hilite();
	}
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
		Irssi::print("IrssiNotifier: Set API token to send notifications: /set irssinotifierwp_api_token [token]");
		return;
	}

	`/usr/bin/env wget --version`;
	if ($? != 0) {
		Irssi::print("IrssiNotifier: You'll need to install Wget to use IrssiNotifier");
		return;
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
		Irssi::print("IrssiNotifier: Api token cannot contain backticks, double quotes or backslashes");
		return;
	}
	
	pipe(my $read_handle, my $write_handle);
	
	my $pid = fork();
	if (!defined($pid)) {
		Irssi::print("Can't fork() - aborting");
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
		
		my $result = `/usr/bin/env wget --no-check-certificate --user-agent="irssinotifierWP7script/$VERSION" -qO- /dev/null $data https://irssinotifierwp.appspot.com/irssi/message`;
		if ($? != 0) {
			# Something went wrong, might be network error or authorization issue. Probably no need to alert user, though.
			# Irssi::print("IrssiNotifier: Sending hilight to server failed, check http://irssinotifierwp.appspot.com for updates");
			# return;
		}
		else{
			if (length($result) > 0) {
				print $write_handle $result;
			}
		#	else{
		#		print $write_handle "Ei ongelmia";
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
		Irssi::print("IrssiNotifier: $line");
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

Irssi::settings_add_str('IrssiNotifierWP', 'irssinotifierwp_api_token', '');
Irssi::settings_add_bool('IrssiNotifierWP', 'irssinotifierwp_away_only', false);
Irssi::settings_add_bool('IrssiNotifierWP', 'irssinotifierwp_ignore_active_window', false);
Irssi::settings_add_int('IrssiNotifierWP', 'irssinotifierwp_require_idle_seconds', 0);

Irssi::signal_add('message irc action', 'public');
Irssi::signal_add('message public', 'public');
Irssi::signal_add('message private', 'private');
Irssi::signal_add('print text', 'print_text');
Irssi::signal_add('setup changed', 'setup_keypress_handler');

setup_keypress_handler();
