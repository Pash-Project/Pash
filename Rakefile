#Add color to setup
class String
  def self.colorize(text, color_code)
    "\e[#{color_code}m#{text}\e[0m"
  end
  def cyan
    self.class.colorize(self, 36)
  end
  def green
    self.class.colorize(self, 32)
  end
end



desc 'Building Solution'
task :build do

  puts 'Building Solution'.cyan
  sh 'xbuild Source/Pash.sln'

  puts 'BUILD COMPLETED!'.green
end

desc 'Running Tests'
task :test => ['build'] do

  puts 'Running Tests'
  sh 'mono Tools/NUnit-2.6.1/bin/nunit-console.exe Pash.nunit'

  puts 'TEST COMPLETE'.green
end
